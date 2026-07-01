using AcademicGateway.Application.Features.ProjectTemplates.Commands.CreateTemplate;
using AcademicGateway.Application.Features.ProjectTemplates.Commands.ReviewTemplate;
using AcademicGateway.Infrastructure.Identity;
using Domain.ProjectTemplates;
using Domain.ProjectTemplates.Enums;
using FluentAssertions;
using Xunit;

namespace AcademicGateway.IntegrationTests.Features.ProjectTemplates;

public class ProjectTemplateWorkflowTests : BaseIntegrationTest
{
    public ProjectTemplateWorkflowTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Should_CreateAndApproveTemplate_WhenProviderIsVerifiedAndCommandIsValid()
    {
        // --- 1. ARRANGE ---
        var providerUserId = Guid.NewGuid().ToString();
        var reviewerIdentityUserId = Guid.NewGuid().ToString();

        var providerUser = new ApplicationUser { Id = providerUserId, UserName = "tmpl_prov", Email = "tmpl_p@test.com" };
        var reviewerUser = new ApplicationUser { Id = reviewerIdentityUserId, UserName = "tmpl_rev", Email = "tmpl_r@test.com" };
        await AddAsync(providerUser);
        await AddAsync(reviewerUser);

        var verifiedProvider = new Provider
        {
            UserId = providerUserId,
            OrganizationName = "Tech Academy",
            Industry = "Education",
            IsVerified = true
        };

        var reviewer = new Reviewer(Guid.NewGuid(), reviewerIdentityUserId, "Curriculum Specialist");
        var skill1 = new Skill { Id = Guid.NewGuid(), Name = "Docker" };
        var skill2 = new Skill { Id = Guid.NewGuid(), Name = "Kubernetes" };

        await AddAsync(verifiedProvider);
        await AddAsync(reviewer);
        await AddAsync(skill1);
        await AddAsync(skill2);

        var createCommand = new CreateProjectTemplateCommand
        {
            ProviderId = providerUserId,
            Title = "Cloud Native Architecture Blueprint",
            Description = "An advanced project detailing containerized microservices deployment workflows.",
            ExpectedDurationWeeks = 6,
            SkillIds = new List<Guid> { skill1.Id, skill2.Id }
        };

        // --- 2. ACT ---
        Guid templateId = await SendAsync(createCommand);

        // --- ASSERT ---
        templateId.Should().NotBeEmpty();
        var templateRecord = await FindAsync<ProjectTemplate>(templateId);
        templateRecord!.Status.Should().Be(ProjectTemplateStatus.PendingReview);

        var reviewCommand = new ReviewProjectTemplateCommand
        {
            TemplateId = templateId,
            ReviewerIdentityUserId = reviewerIdentityUserId,
            IsApproved = true,
            RejectionReason = null
        };

        await SendAsync(reviewCommand);

        var approvedTemplate = await FindAsync<ProjectTemplate>(templateId);
        approvedTemplate!.Status.Should().Be(ProjectTemplateStatus.Approved);
        approvedTemplate.ApprovedById.Should().Be(reviewer.Id);
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_WhenUnverifiedProviderAttemptsTemplateCreation()
    {
        var providerUserId = Guid.NewGuid().ToString();

        var providerUser = new ApplicationUser { Id = providerUserId, UserName = "unv_tmpl", Email = "unv_t@test.com" };
        await AddAsync(providerUser);

        var unverifiedProvider = new Provider { UserId = providerUserId, OrganizationName = "Rogue Org Inc", Industry = "Education", IsVerified = false };
        var dummySkill = new Skill { Id = Guid.NewGuid(), Name = "Dummy" };
        await AddAsync(unverifiedProvider);
        await AddAsync(dummySkill);

        var command = new CreateProjectTemplateCommand
        {
            ProviderId = providerUserId,
            Title = "Unauthorized Project Template",
            Description = "This attempt should be blocked by domain guards.",
            ExpectedDurationWeeks = 4,
            SkillIds = new List<Guid> { dummySkill.Id }
        };

        Func<Task> act = async () => await SendAsync(command);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_ThrowKeyNotFoundException_WhenProviderProfileDoesNotExist()
    {
        var dummySkill = new Skill { Id = Guid.NewGuid(), Name = "Valid Skill" };
        await AddAsync(dummySkill);

        var command = new CreateProjectTemplateCommand
        {
            ProviderId = Guid.NewGuid().ToString(),
            Title = "Ghost Template Integration Unit",
            Description = "No provider matches this profile reference input parameters.",
            ExpectedDurationWeeks = 2,
            SkillIds = new List<Guid> { dummySkill.Id }
        };

        Func<Task> act = async () => await SendAsync(command);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Should_RejectTemplateAndLogReason_WhenReviewerRejects()
    {
        var providerUserId = Guid.NewGuid().ToString();
        var reviewerIdentityUserId = Guid.NewGuid().ToString();

        var providerUser = new ApplicationUser { Id = providerUserId, UserName = "rej_t_p", Email = "rej_tp@test.com" };
        var reviewerUser = new ApplicationUser { Id = reviewerIdentityUserId, UserName = "rej_t_r", Email = "rej_tr@test.com" };
        await AddAsync(providerUser);
        await AddAsync(reviewerUser);

        var provider = new Provider { UserId = providerUserId, OrganizationName = "Test", Industry = "Tech", IsVerified = true };
        var reviewer = new Reviewer(Guid.NewGuid(), reviewerIdentityUserId, "Strict Inspector");
        var dummySkill = new Skill { Id = Guid.NewGuid(), Name = "Skill" };
        await AddAsync(provider);
        await AddAsync(reviewer);
        await AddAsync(dummySkill);

        var createCommand = new CreateProjectTemplateCommand
        {
            ProviderId = providerUserId,
            Title = "Poorly Structured Syllabus",
            Description = "Lacks standard engineering metrics.",
            ExpectedDurationWeeks = 1,
            SkillIds = new List<Guid> { dummySkill.Id }
        };

        Guid templateId = await SendAsync(createCommand);

        var rejectCommand = new ReviewProjectTemplateCommand
        {
            TemplateId = templateId,
            ReviewerIdentityUserId = reviewerIdentityUserId,
            IsApproved = false,
            RejectionReason = "The curriculum scope is entirely too brief for our standards."
        };

        await SendAsync(rejectCommand);

        var rejectedTemplate = await FindAsync<ProjectTemplate>(templateId);
        rejectedTemplate!.Status.Should().Be(ProjectTemplateStatus.Rejected);
        rejectedTemplate.RejectionReason.Should().Be("The curriculum scope is entirely too brief for our standards.");
    }
}
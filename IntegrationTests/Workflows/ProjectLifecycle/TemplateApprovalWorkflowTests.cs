using AcademicGateway.Application.Features.ProjectTemplates.Commands.CreateProjectTemplate;
using AcademicGateway.Application.Features.ProjectTemplates.Commands.ReviewProjectTemplate;
using AcademicGateway.Application.Features.Providers.Commands.RegisterProvider;
using AcademicGateway.Domain.ProjectTemplates;
using AcademicGateway.Domain.ProjectTemplates.Enums;
using AcademicGateway.Domain.Reviewers;
using AcademicGateway.Domain.Skills;
using AcademicGateway.Domain.Providers;
using AcademicGateway.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using IntegrationTests.Infrastructure;

namespace AcademicGateway.IntegrationTests.Workflows.ProjectLifecycle;

/// <summary>
/// End-to-end workflow integration tests validating the macro project template lifecycle saga
/// from initial provider proposal submission through successful faculty evaluation and approval.
/// </summary>
[Collection("SharedDatabase")]
public class TemplateApprovalWorkflowTests : BaseIntegrationTest
{
    private readonly IServiceScopeFactory _scopeFactory;

    public TemplateApprovalWorkflowTests(CustomWebApplicationFactory factory) : base(factory)
    {
        _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    /// <summary>
    /// Validates the primary happy path scenario where a verified corporate provider proposes a template
    /// with required skill sets, and a faculty reviewer reviews and approves it into the active pool.
    /// </summary>
    [Fact]
    public async Task Should_CreateAndApproveTemplate_WhenProviderIsVerifiedAndCommandIsValid()
    {
        // ==========================================
        // 1. ARRANGE & SEED SYSTEM ANTECEDENTS
        // ==========================================

        // Register the corporate provider account profile context
        var providerCommand = new RegisterProviderCommand
        {
            Email = "workflow.approval.prov@academicgateway.com",
            Username = "approvalprov",
            Password = "SecurePassword123!",
            CompanyName = "Tech Academy Labs",
            CompanyDescription = "Advanced Software Engineering Hub",
            WebsiteUrl = "https://tech-academy-labs.org"
        };
        Guid providerId = await SendAsync(providerCommand);

        // Administrative Step: Simulate provider background verification via rich domain actions
        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var providerAggregate = await context.Providers.FindAsync(new object[] { providerId }, TestContext.Current.CancellationToken);
            providerAggregate.Should().NotBeNull();

            // Invoke correct rich domain behavior to upgrade verification status standing
            providerAggregate!.VerifyProfile();
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // Register an evaluator profile to act as our faculty curriculum specialist
        var reviewer = new Reviewer(Guid.NewGuid(), "Curriculum Specialist");
        await AddAsync(reviewer);

        // Seed the standard lookup competencies required for the upcoming proposal
        var skill1 = new Skill("Docker");
        var skill2 = new Skill("Kubernetes");
        await AddAsync(skill1);
        await AddAsync(skill2);

        // Prepare the proposal creation command matching the refactored schema (duration removed)
        var createTemplateCommand = new CreateProjectTemplateCommand
        {
            ProviderId = providerId,
            Title = "Cloud Native Architecture Blueprint",
            Description = "An advanced project detailing containerized microservices deployment workflows.",
            SkillIds = new List<Guid> { skill1.Id, skill2.Id }
        };

        // ==========================================
        // 2. ACT (STAGE A): SUBMIT PROPOSAL
        // ==========================================
        Guid templateId = await SendAsync(createTemplateCommand);

        // Assert intermediate state: Proposal must transition directly to a pending review pool
        templateId.Should().NotBeEmpty();
        var intermediateRecord = await FindAsync<ProjectTemplate>(templateId);
        intermediateRecord.Should().NotBeNull();
        intermediateRecord!.Status.Should().Be(ProjectTemplateStatus.PendingReview);

        // Prepare the review evaluation command to approve the proposal concept
        var reviewCommand = new ReviewProjectTemplateCommand
        {
            TemplateId = templateId,
            ReviewerId = reviewer.Id,
            IsApproved = true,
            RejectionReason = null
        };

        // ==========================================
        // 3. ACT (STAGE B): EVALUATE & APPROVE
        // ==========================================
        await SendAsync(reviewCommand);

        // ==========================================
        // 4. ASSERT FINAL SYSTEM STATE
        // ==========================================
        var finalApprovedTemplate = await FindAsync<ProjectTemplate>(templateId);
        finalApprovedTemplate.Should().NotBeNull();

        // Verify that the template state machine successfully updated to Approved status
        finalApprovedTemplate!.Status.Should().Be(ProjectTemplateStatus.Approved);
    }
}
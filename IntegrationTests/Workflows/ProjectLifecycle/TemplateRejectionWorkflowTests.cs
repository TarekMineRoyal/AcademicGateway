using AcademicGateway.Application.Features.ProjectTemplates.Commands.CreateProjectTemplate;
using AcademicGateway.Application.Features.ProjectTemplates.Commands.ReviewProjectTemplate;
using AcademicGateway.Application.Features.Providers.Commands.RegisterProvider;
using AcademicGateway.Application.Features.ProviderApplications.Commands.ReviewProviderApplication;
using AcademicGateway.Application.Features.ProviderApplications.Commands.SubmitProviderApplication;
using AcademicGateway.Domain.ProjectTemplates;
using AcademicGateway.Domain.ProjectTemplates.Enums;
using AcademicGateway.Domain.Reviewers;
using AcademicGateway.Infrastructure.Identity;
using FluentAssertions;
using IntegrationTests.Infrastructure;
using System;
using System.Threading.Tasks;
using Xunit;
using AcademicGateway.Domain.Skills;

namespace AcademicGateway.IntegrationTests.Workflows.ProjectLifecycle;

/// <summary>
/// End-to-end workflow integration tests validating project template rejection paths,
/// administrative evaluations, and preservation of the draft status or transition to rejected.
/// </summary>
[Collection("SharedDatabase")]
public class TemplateRejectionWorkflowTests : BaseIntegrationTest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateRejectionWorkflowTests"/> class.
    /// </summary>
    /// <param name="factory">The centralized integration web application testing factory infrastructure context.</param>
    public TemplateRejectionWorkflowTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Validates the workflow where a reviewer rejects a project template, ensuring the 
    /// status transitions correctly and the reason is persisted.
    /// </summary>
    [Fact]
    public async Task Should_RejectTemplateAndLogReason_WhenReviewerRejects()
    {
        // ==========================================
        // 1. ARRANGE
        // ==========================================
        var registerProviderCommand = new RegisterProviderCommand
        {
            Email = "workflow.rejection.prov@academicgateway.com",
            Username = "rejectionprov",
            Password = "SecurePassword123!",
            CompanyName = "Syllabus Testing Corp",
            CompanyDescription = "External Engineering Curriculum Verification Labs",
            WebsiteUrl = "https://syllabus-test.org"
        };
        Guid providerId = await SendAsync(registerProviderCommand);

        // Provision User Identity for Reviewer
        var reviewerUser = new ApplicationUser { Id = Guid.NewGuid(), UserName = "auditor@gateway.com", Email = "auditor@gateway.com" };
        await AddAsync(reviewerUser);
        var reviewer = new Reviewer(reviewerUser.Id, "Strict Inspector");
        await AddAsync(reviewer);

        // --- COMPLETE VERIFICATION WORKFLOW TO ALLOW TEMPLATE CREATION ---
        var submitAppCommand = new SubmitProviderApplicationCommand { ProviderId = providerId,
            CompanyDetails = "Dossier requiring professional audit and compliance check", VerificationDocumentsUrl = "https://docs.com/v.pdf" };
            Guid applicationId = await SendAsync(submitAppCommand);

        await SendAsync(new ReviewProviderApplicationCommand { ApplicationId = applicationId, ReviewerId = reviewer.Id, IsApproved = true });

        // Seed dummy skill
        var dummySkill = new Skill("Architecture Metrics");
        await AddAsync(dummySkill);

        // Now template creation will succeed
        var createTemplateCommand = new CreateProjectTemplateCommand
        {
            ProviderId = providerId,
            Title = "Poorly Structured Syllabus Concept",
            Description = "A brief summary lacking clear operational milestones and evaluation keys.",
            SkillIds = new List<Guid> { dummySkill.Id }
        };
        Guid templateId = await SendAsync(createTemplateCommand);

        // ==========================================
        // 2. ACT
        // ==========================================
        const string rejectionReasonText = "The curriculum scope is entirely too brief.";
        var rejectTemplateCommand = new ReviewProjectTemplateCommand
        {
            TemplateId = templateId,
            ReviewerId = reviewer.Id,
            IsApproved = false,
            RejectionReason = rejectionReasonText
        };
        await SendAsync(rejectTemplateCommand);

        // ==========================================
        // 3. ASSERT
        // ==========================================
        var rejectedTemplate = await FindAsync<ProjectTemplate>(templateId);
        rejectedTemplate.Should().NotBeNull();
        rejectedTemplate!.Status.Should().Be(ProjectTemplateStatus.Rejected);
        rejectedTemplate.ReviewerFeedback.Should().Be(rejectionReasonText);
    }
}
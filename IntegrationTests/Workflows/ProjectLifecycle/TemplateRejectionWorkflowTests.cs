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
/// End-to-end workflow integration tests validating the negative branches of the macro project 
/// template lifecycle saga, specifically focusing on administrative rejections and auditing.
/// </summary>
[Collection("SharedDatabase")]
public class TemplateRejectionWorkflowTests : BaseIntegrationTest
{
    private readonly IServiceScopeFactory _scopeFactory;

    public TemplateRejectionWorkflowTests(CustomWebApplicationFactory factory) : base(factory)
    {
        _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    /// <summary>
    /// Validates that when an active proposal fails to satisfy curriculum criteria, 
    /// a faculty evaluator can reject the template, successfully updating its lifecycle status 
    /// and persisting the mandatory justification feedback text.
    /// </summary>
    [Fact]
    public async Task Should_RejectTemplateAndLogReason_WhenReviewerRejects()
    {
        // ==========================================
        // 1. ARRANGE & SEED SYSTEM ANTECEDENTS
        // ==========================================

        // Register the corporate provider account profile context
        var providerCommand = new RegisterProviderCommand
        {
            Email = "workflow.rejection.prov@academicgateway.com",
            Username = "rejectionprov",
            Password = "SecurePassword123!",
            CompanyName = "Syllabus Testing Corp",
            CompanyDescription = "External Engineering Curriculum Verification Labs",
            WebsiteUrl = "https://syllabus-test.org"
        };
        Guid providerId = await SendAsync(providerCommand);

        // Administrative Step: Simulate provider background verification via rich domain actions
        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var providerAggregate = await context.Providers.FindAsync(new object[] { providerId }, TestContext.Current.CancellationToken);
            providerAggregate.Should().NotBeNull();

            // Upgrade verification standing using the exact domain name vector to allow project creation actions
            providerAggregate!.VerifyProfile();
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // Register a faculty evaluator profile to audit the proposal entry
        var reviewer = new Reviewer(Guid.NewGuid(), "Strict Inspector");
        await AddAsync(reviewer);

        // Seed lookups using rich domain constructor patterns
        var dummySkill = new Skill("Software Architecture Metrics");
        await AddAsync(dummySkill);

        // Build the creation command matching the refactored schema boundaries
        var createTemplateCommand = new CreateProjectTemplateCommand
        {
            ProviderId = providerId,
            Title = "Poorly Structured Syllabus Concept",
            Description = "A brief summary lacking clear operational milestones and evaluation keys.",
            SkillIds = new List<Guid> { dummySkill.Id }
        };

        // Dispatch the proposal to populate our pending evaluation queue
        Guid templateId = await SendAsync(createTemplateCommand);

        // Set up the review evaluation command to flag a rejection tracking sequence
        const string rejectionReasonText = "The curriculum scope is entirely too brief and lacks required academic depth.";
        var rejectCommand = new ReviewProjectTemplateCommand
        {
            TemplateId = templateId,
            ReviewerId = reviewer.Id,
            IsApproved = false,
            RejectionReason = rejectionReasonText
        };

        // ==========================================
        // 2. ACT - EXECUTE EVALUATION AUDIT
        // ==========================================
        await SendAsync(rejectCommand);

        // ==========================================
        // 3. ASSERT FINAL SYSTEM STATE
        // ==========================================
        var rejectedTemplate = await FindAsync<ProjectTemplate>(templateId);
        rejectedTemplate.Should().NotBeNull();

        // Verify that the template state machine transitioned to a terminal rejected standing
        rejectedTemplate!.Status.Should().Be(ProjectTemplateStatus.Rejected);

        // Verify that audit feedback documentation was saved successfully using the refactored property name
        rejectedTemplate.ReviewerFeedback.Should().Be(rejectionReasonText);
    }
}
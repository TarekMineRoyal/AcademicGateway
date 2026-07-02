using AcademicGateway.Application.Features.ProjectTemplates.Commands.CreateProjectTemplate;
using AcademicGateway.Application.Features.ProjectTemplates.Commands.ReviewProjectTemplate;
using AcademicGateway.Application.Features.Providers.Commands.RegisterProvider;
using AcademicGateway.Domain.ProjectTemplates;
using AcademicGateway.Domain.ProjectTemplates.Enums;
using AcademicGateway.Domain.Reviewers;
using AcademicGateway.Domain.Skills;
using AcademicGateway.Domain.Providers;
using AcademicGateway.Infrastructure.Persistence;
using AcademicGateway.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using IntegrationTests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcademicGateway.IntegrationTests.Workflows.ProjectLifecycle;

/// <summary>
/// End-to-end workflow integration tests validating the macro project template lifecycle saga
/// from initial provider proposal submission through successful faculty evaluation and approval.
/// </summary>
[Collection("SharedDatabase")]
public class TemplateApprovalWorkflowTests : BaseIntegrationTest
{
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateApprovalWorkflowTests"/> class.
    /// </summary>
    /// <param name="factory">The centralized integration web application testing factory infrastructure context.</param>
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

        // Administrative Step: Simulate provider background verification
        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var providerAggregate = await context.Providers.FindAsync(new object[] { providerId }, TestContext.Current.CancellationToken);
            providerAggregate!.VerifyProfile();
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // Provision the underlying user identity security row first to satisfy 1:1 relational constraints
        var reviewerUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "auditor.template@academicgateway.com",
            Email = "auditor.template@academicgateway.com"
        };
        await AddAsync(reviewerUser);

        // Register an evaluator profile to act as our faculty curriculum specialist
        var reviewer = new Reviewer(reviewerUser.Id, "Curriculum Specialist");
        await AddAsync(reviewer);

        // Seed the standard lookup competencies required for the upcoming proposal
        var skill1 = new Skill("Docker");
        var skill2 = new Skill("Kubernetes");
        await AddAsync(skill1);
        await AddAsync(skill2);

        // Prepare the proposal creation command
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
        finalApprovedTemplate!.Status.Should().Be(ProjectTemplateStatus.Approved);
    }
}
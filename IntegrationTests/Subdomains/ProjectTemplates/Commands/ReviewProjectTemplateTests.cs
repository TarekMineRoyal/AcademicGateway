using AcademicGateway.Application.Features.ProjectTemplates.Commands.ReviewProjectTemplate;
using AcademicGateway.Application.Features.Providers.Commands.RegisterProvider;
using AcademicGateway.Domain.ProjectTemplates;
using AcademicGateway.Domain.ProjectTemplates.Exceptions;
using AcademicGateway.Domain.Reviewers;
using AcademicGateway.Infrastructure.Identity;
using FluentAssertions;
using IntegrationTests.Infrastructure;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.IntegrationTests.Subdomains.ProjectTemplates.Commands;

/// <summary>
/// Integration tests verifying workflow transitions, domain invariant validations,
/// and evaluation processing rules handled by the review project template command pipeline.
/// </summary>
[Collection("SharedDatabase")]
public class ReviewProjectTemplateTests : BaseIntegrationTest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReviewProjectTemplateTests"/> class.
    /// </summary>
    /// <param name="factory">The centralized integration web application testing factory infrastructure context.</param>
    public ReviewProjectTemplateTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Ensures that an evaluator attempting to approve a project proposal that resides strictly 
    /// in a Draft status is blocked, throwing an explicit <see cref="InvalidTemplateStatusException"/>
    /// because the blueprint has not been submitted into the pending evaluation pool.
    /// </summary>
    [Fact]
    public async Task Should_ThrowInvalidTemplateStatusException_WhenReviewerAttemptsToEvaluateDraftTemplate()
    {
        // --- 1. ARRANGE ---
        // Register a corporate partner provider account profile context
        var registerProviderCommand = new RegisterProviderCommand
        {
            Email = "template.state@academicgateway.com",
            Username = "templatestateprov",
            Password = "SecurePassword123!",
            CompanyName = "Template Guard LLC",
            CompanyDescription = "Academic Research Curriculum Integrity Solutions",
            WebsiteUrl = "https://template-guard-llc.com"
        };
        Guid providerId = await SendAsync(registerProviderCommand);

        // Provision the underlying user identity security row first to satisfy 1:1 relational constraints
        var reviewerUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "template.auditor@academicgateway.com",
            Email = "template.auditor@academicgateway.com"
        };
        await AddAsync(reviewerUser);

        // Instantiate an evaluator profile using rich domain aggregate patterns mapped to the user context
        var reviewer = new Reviewer(reviewerUser.Id, "Template Auditor");
        await AddAsync(reviewer);

        // Explicitly instantiate a template using the rich domain constructor pattern.
        // This naturally defaults its internal lifecycle status flag directly to Draft mode.
        var draftTemplate = new ProjectTemplate(
            title: "Illegal State Test",
            description: "Testing workflow boundary constraints preventing premature approvals.",
            providerId: providerId
        );

        // Save the draft template to the database, deliberately skipping the .SubmitForReview() step
        await AddAsync(draftTemplate);

        // Prepare the review action command targeting the unsubmitted draft concept
        var illegalReviewCommand = new ReviewProjectTemplateCommand
        {
            TemplateId = draftTemplate.Id,
            ReviewerId = reviewer.Id,
            IsApproved = true,
            RejectionReason = null
        };

        // --- 2. ACT ---
        // Capture the asynchronous execution logic into a delegate for exception verification
        Func<Task> act = async () => await SendAsync(illegalReviewCommand);

        // --- 3. ASSERT ---
        // Assert that the domain layer's state-machine boundary successfully intercepts and blocks the illegal mutation
        await act.Should().ThrowAsync<InvalidTemplateStatusException>();
    }
}
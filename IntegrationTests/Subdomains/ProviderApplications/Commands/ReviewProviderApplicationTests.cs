using AcademicGateway.Application.Features.ProviderApplications.Commands.ReviewProviderApplication;
using AcademicGateway.Application.Features.Providers.Commands.RegisterProvider;
using AcademicGateway.Domain.Providers;
using AcademicGateway.Domain.Providers.Exceptions;
using AcademicGateway.Domain.Reviewers;
using AcademicGateway.Infrastructure.Identity;
using FluentAssertions;
using IntegrationTests.Infrastructure;
using System;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationTests.Subdomains.ProviderApplications.Commands;

/// <summary>
/// Integration tests verifying workflow transitions, domain aggregate boundaries,
/// and evaluation ordering policies handled by the review provider application command pipeline.
/// </summary>
[Collection("SharedDatabase")]
public class ReviewProviderApplicationTests : BaseIntegrationTest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReviewProviderApplicationTests"/> class.
    /// </summary>
    /// <param name="factory">The centralized integration web application testing factory infrastructure context.</param>
    public ReviewProviderApplicationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Ensures that an administrator attempting to approve a provider enrollment application that 
    /// is still in a Draft status is blocked, throwing an explicit <see cref="InvalidApplicationStatusException"/>.
    /// </summary>
    [Fact]
    public async Task Should_ThrowInvalidApplicationStatusException_WhenReviewerAttemptsToApproveDraftApplication()
    {
        // --- 1. ARRANGE ---
        // Register corporate provider to spawn a clean identity user context record
        var registerProviderCommand = new RegisterProviderCommand
        {
            Email = "statetest.provider1@academicgateway.com",
            Username = "stateprovider1",
            Password = "SecurePassword123!",
            CompanyName = "State Test Co",
            CompanyDescription = "Global Enterprise Logistics Infrastructure Solutions",
            WebsiteUrl = "https://state-test-logistics.com"
        };
        Guid providerId = await SendAsync(registerProviderCommand);

        // Provision the underlying user identity security row first to satisfy 1:1 relational constraints
        var reviewerUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "guard.reviewer1@academicgateway.com",
            Email = "guard.reviewer1@academicgateway.com"
        };
        await AddAsync(reviewerUser);

        // Seed an active institutional reviewer profile context using the valid Identity reference key
        var reviewer = new Reviewer(reviewerUser.Id, "Guard Reviewer");
        await AddAsync(reviewer);

        // Instantiate the application via its rich constructor, which naturally defaults status to Draft mode
        var application = new ProviderApplication(
            providerId: providerId,
            companyDetails: "Company Logistics Information Summary Dossier",
            verificationDocumentsUrl: "https://docs.com/logistics-verification.pdf",
            createdAt: DateTime.UtcNow
        );

        // Persist the record in Draft mode, deliberately skipping the .SubmitForReview() step
        await AddAsync(application);

        // Build the review evaluation command targeting the unsubmitted draft enrollment application
        var badReviewCommand = new ReviewProviderApplicationCommand
        {
            ApplicationId = application.Id,
            ReviewerId = reviewer.Id,
            IsApproved = true,
            RejectionReason = null
        };

        // --- 2. ACT ---
        // Capture execution tracking delegate for exception pattern checking
        Func<Task> act = async () => await SendAsync(badReviewCommand);

        // --- 3. ASSERT ---
        // Assert that the rich domain aggregate model intercepts the invalid execution flow
        await act.Should().ThrowAsync<InvalidApplicationStatusException>();
    }

    /// <summary>
    /// Ensures that an administrator attempting to log a rejection decision against an application 
    /// that has already completed evaluation and reached an Approved status is explicitly blocked.
    /// </summary>
    [Fact]
    public async Task Should_ThrowInvalidApplicationStatusException_WhenReviewerAttemptsToRejectAlreadyApprovedApplication()
    {
        // --- 1. ARRANGE ---
        var registerProviderCommand = new RegisterProviderCommand
        {
            Email = "statetest.provider2@academicgateway.com",
            Username = "stateprovider2",
            Password = "SecurePassword123!",
            CompanyName = "State Test Corp",
            CompanyDescription = "Automated High Frequency FinTech Systems",
            WebsiteUrl = "https://state-test-fintech.com"
        };
        Guid providerId = await SendAsync(registerProviderCommand);

        // Provision the underlying user identity security row first to satisfy 1:1 relational constraints
        var reviewerUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "guard.reviewer2@academicgateway.com",
            Email = "guard.reviewer2@academicgateway.com"
        };
        await AddAsync(reviewerUser);

        var reviewer = new Reviewer(reviewerUser.Id, "Guard Reviewer Two");
        await AddAsync(reviewer);

        // Instantiate the application aggregate following explicit domain rules
        var application = new ProviderApplication(
            providerId: providerId,
            companyDetails: "Corporate Banking Credentials and Entity Registration Ledger",
            verificationDocumentsUrl: "https://docs.com/fintech-verification.pdf",
            createdAt: DateTime.UtcNow
        );

        // Execute valid forward state-machine transitions to simulate a completed happy path
        application.SubmitForReview();
        application.Approve(reviewer.Id, DateTime.UtcNow);
        await AddAsync(application);

        // Attempt a conflicting execution flow by trying to reject a closed, approved profile application
        var illegalRejectCommand = new ReviewProviderApplicationCommand
        {
            ApplicationId = application.Id,
            ReviewerId = reviewer.Id,
            IsApproved = false,
            RejectionReason = "Changing internal evaluation metrics retroactively."
        };

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(illegalRejectCommand);

        // --- 3. ASSERT ---
        // Verify the domain aggregate guards block rewriting historical application milestones
        await act.Should().ThrowAsync<InvalidApplicationStatusException>();
    }
}
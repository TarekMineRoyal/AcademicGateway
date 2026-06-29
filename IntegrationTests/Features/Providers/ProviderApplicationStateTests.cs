using AcademicGateway.Application.Features.Reviewers.Commands.ReviewApplication;
using AcademicGateway.Application.Features.Users.Commands.RegisterProvider;
using AcademicGateway.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace AcademicGateway.IntegrationTests.Features.Providers;

public class ProviderApplicationStateTests : BaseIntegrationTest
{
    public ProviderApplicationStateTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_WhenReviewerAttemptsToApproveDraftApplication()
    {
        // --- 1. ARRANGE ---
        // Register provider to get a clean Identity User entry
        var registerProviderCommand = new RegisterProviderCommand
        {
            Email = "statetest.provider1@academicgateway.com",
            Username = "stateprovider1",
            Password = "SecurePassword123!",
            OrganizationName = "State Test Co",
            Industry = "Logistics"
        };
        var providerId = await SendAsync(registerProviderCommand);

        // Seed a Reviewer using the providerId as IdentityUserId to satisfy relational keys
        var reviewerId = Guid.NewGuid();
        var reviewer = new Reviewer(reviewerId, providerId, "Guard Reviewer");
        await AddAsync(reviewer);

        // Intentionally save a ProviderApplication in its raw 'Draft' state
        var application = new ProviderApplication(providerId, "Company Info", "https://docs.com");
        await AddAsync(application);

        // Attempting to evaluate it before it is submitted for review
        var badReviewCommand = new ReviewProviderApplicationCommand
        {
            ApplicationId = application.Id,
            ReviewerIdentityUserId = providerId, // Matching the reviewer profile setup above
            IsApproved = true,
            RejectionReason = null
        };

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(badReviewCommand);

        // --- 3. ASSERT ---
        // Verify that the domain state guard successfully threw an InvalidOperationException
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("Only pending applications can be approved.");
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_WhenReviewerAttemptsToRejectAlreadyApprovedApplication()
    {
        // --- 1. ARRANGE ---
        var registerProviderCommand = new RegisterProviderCommand
        {
            Email = "statetest.provider2@academicgateway.com",
            Username = "stateprovider2",
            Password = "SecurePassword123!",
            OrganizationName = "State Test Corp",
            Industry = "FinTech"
        };
        var providerId = await SendAsync(registerProviderCommand);

        var reviewerId = Guid.NewGuid();
        var reviewer = new Reviewer(reviewerId, providerId, "Guard Reviewer Two");
        await AddAsync(reviewer);

        // Construct application, submit it, approve it, and write it to the DB
        var application = new ProviderApplication(providerId, "Company Info", "https://docs.com");
        application.SubmitForReview();
        application.Approve(reviewerId);
        await AddAsync(application);

        // Attempt to maliciously reject it after it has already achieved Approved status
        var illegalRejectCommand = new ReviewProviderApplicationCommand
        {
            ApplicationId = application.Id,
            ReviewerIdentityUserId = providerId,
            IsApproved = false,
            RejectionReason = "Changing my mind."
        };

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(illegalRejectCommand);

        // --- 3. ASSERT ---
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("Only pending applications can be rejected.");
    }
}
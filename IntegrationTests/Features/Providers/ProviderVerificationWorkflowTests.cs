using AcademicGateway.Application.Features.Providers.Commands.SubmitApplication;
using AcademicGateway.Application.Features.Reviewers.Commands.ReviewApplication;
using AcademicGateway.Domain.Entities;
using AcademicGateway.Domain.Enums;
using AcademicGateway.Infrastructure.Identity;
using FluentAssertions;
using FluentValidation;
using Xunit;

namespace AcademicGateway.IntegrationTests.Features.Providers;

public class ProviderVerificationWorkflowTests : BaseIntegrationTest
{
    public ProviderVerificationWorkflowTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Should_CompleteFullVerificationWorkflow_WhenApplicationIsSubmittedAndApproved()
    {
        // --- 1. ARRANGE ---
        var providerUserId = Guid.NewGuid().ToString();
        var reviewerIdentityUserId = Guid.NewGuid().ToString();

        var providerUser = new ApplicationUser { Id = providerUserId, UserName = "prov_user", Email = "prov@test.com" };
        var reviewerUser = new ApplicationUser { Id = reviewerIdentityUserId, UserName = "rev_user", Email = "rev@test.com" };
        await AddAsync(providerUser);
        await AddAsync(reviewerUser);

        var rawProvider = new Provider { UserId = providerUserId, IsVerified = false };
        var reviewer = new Reviewer(Guid.NewGuid(), reviewerIdentityUserId, "Test Reviewer");
        await AddAsync(rawProvider);
        await AddAsync(reviewer);

        var submitCommand = new SubmitProviderApplicationCommand
        {
            ProviderId = providerUserId,
            CompanyDetails = "EdTech Solutions Corp",
            VerificationDocumentsUrl = "https://documents.academicgateway.com/edtech-verify.pdf"
        };

        // --- 2. ACT ---
        Guid applicationId = await SendAsync(submitCommand);

        // --- ASSERT ---
        applicationId.Should().NotBeEmpty();
        var appRecord = await FindAsync<ProviderApplication>(applicationId);
        appRecord.Should().NotBeNull();
        appRecord!.Status.Should().Be(ProviderApplicationStatus.PendingReview);

        var reviewCommand = new ReviewProviderApplicationCommand
        {
            ApplicationId = applicationId,
            ReviewerIdentityUserId = reviewerIdentityUserId,
            IsApproved = true,
            RejectionReason = null
        };

        await SendAsync(reviewCommand);

        var updatedAppRecord = await FindAsync<ProviderApplication>(applicationId);
        updatedAppRecord!.Status.Should().Be(ProviderApplicationStatus.Approved);
        updatedAppRecord.ReviewedById.Should().Be(reviewer.Id);

        var verifiedProvider = await FindAsync<Provider>(providerUserId);
        verifiedProvider!.IsVerified.Should().BeTrue();
    }

    [Fact]
    public async Task Should_ThrowValidationException_WhenSubmittingForNonExistentProvider()
    {
        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = Guid.NewGuid().ToString(),
            CompanyDetails = "Ghost Company LLC",
            VerificationDocumentsUrl = "https://verify.com/ghost.pdf"
        };

        Func<Task> act = async () => await SendAsync(command);

        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.Any(e => e.ErrorMessage.Contains("specified Provider profile does not exist")));
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_WhenProviderAttemptsDuplicateSubmission()
    {
        // --- 1. ARRANGE ---
        var providerUserId = Guid.NewGuid().ToString();

        var providerUser = new ApplicationUser { Id = providerUserId, UserName = "dup_prov", Email = "dup@test.com" };
        await AddAsync(providerUser);

        var provider = new Provider { UserId = providerUserId, IsVerified = false, OrganizationName = "Test Org", Industry = "Tech" };
        await AddAsync(provider);

        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = providerUserId,
            CompanyDetails = "Duplicate Testing Inc",
            VerificationDocumentsUrl = "https://verify.com/docs.pdf"
        };

        await SendAsync(command);

        // --- 2. ACT & 3. ASSERT ---
        Func<Task> act = async () => await SendAsync(command);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*already has an active onboarding application*");
    }

    [Fact]
    public async Task Should_RejectApplicationAndLeaveProviderUnverified_WhenReviewerRejects()
    {
        // --- 1. ARRANGE ---
        var providerUserId = Guid.NewGuid().ToString();
        var reviewerIdentityUserId = Guid.NewGuid().ToString();

        var providerUser = new ApplicationUser { Id = providerUserId, UserName = "rej_prov", Email = "rej_prov@test.com" };
        var reviewerUser = new ApplicationUser { Id = reviewerIdentityUserId, UserName = "rej_rev", Email = "rej_rev@test.com" };
        await AddAsync(providerUser);
        await AddAsync(reviewerUser);

        var provider = new Provider { UserId = providerUserId, IsVerified = false };
        var reviewer = new Reviewer(Guid.NewGuid(), reviewerIdentityUserId, "Test Reviewer");
        await AddAsync(provider);
        await AddAsync(reviewer);

        var submitCommand = new SubmitProviderApplicationCommand
        {
            ProviderId = providerUserId,
            CompanyDetails = "Faulty Docs Corp",
            VerificationDocumentsUrl = "https://verify.com/bad-docs.pdf"
        };

        Guid applicationId = await SendAsync(submitCommand);

        var rejectCommand = new ReviewProviderApplicationCommand
        {
            ApplicationId = applicationId,
            ReviewerIdentityUserId = reviewerIdentityUserId,
            IsApproved = false,
            RejectionReason = "Documents provided are completely illegible."
        };

        await SendAsync(rejectCommand);

        var application = await FindAsync<ProviderApplication>(applicationId);
        application!.Status.Should().Be(ProviderApplicationStatus.Rejected);
        application.RejectionReason.Should().Be("Documents provided are completely illegible.");

        var verifiedProvider = await FindAsync<Provider>(providerUserId);
        verifiedProvider!.IsVerified.Should().BeFalse();
    }
}
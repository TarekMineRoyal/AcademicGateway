using AcademicGateway.Application.Features.ProviderApplications.Commands.ReviewProviderApplication;
using AcademicGateway.Application.Features.ProviderApplications.Commands.SubmitProviderApplication;
using AcademicGateway.Application.Features.Providers.Commands.RegisterProvider;
using AcademicGateway.Domain.Providers;
using AcademicGateway.Domain.Providers.Enums;
using AcademicGateway.Domain.Reviewers;
using FluentAssertions;
using IntegrationTests.Infrastructure;
using Xunit;

namespace AcademicGateway.IntegrationTests.Workflows.OnboardingAndVerification;

/// <summary>
/// End-to-end workflow integration tests validating successful partner onboarding paths,
/// administrative evaluations, and automatic profile verification state updates.
/// </summary>
[Collection("SharedDatabase")]
public class ProviderVerificationWorkflowTests : BaseIntegrationTest
{
    public ProviderVerificationWorkflowTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Validates the full happy path onboarding saga where an unverified company profile submits compliance 
    /// paperwork, an institutional auditor approves it, and the underlying provider profile transitions to verified.
    /// </summary>
    [Fact]
    public async Task Should_CompleteFullVerificationWorkflow_WhenApplicationIsSubmittedAndApproved()
    {
        // ==========================================
        // 1. ARRANGE & INITIALIZE WORKFLOW
        // ==========================================

        // Register a corporate partner profile via the pipeline (natively initializes as unverified)
        var registerProviderCommand = new RegisterProviderCommand
        {
            Email = "workflow.verification.prov@academicgateway.com",
            Username = "verificationprov",
            Password = "SecurePassword123!",
            CompanyName = "EdTech Solutions Corp",
            CompanyDescription = "Advanced Academic Integration and Technology Systems",
            WebsiteUrl = "https://edtech-solutions-corp.com"
        };
        Guid providerId = await SendAsync(registerProviderCommand);

        // Pre-assertion: Verify the profile exists and is unverified before submission
        var initialProviderState = await FindAsync<Provider>(providerId);
        initialProviderState.Should().NotBeNull();
        initialProviderState!.IsVerified.Should().BeFalse();

        // Instantiate an official faculty reviewer aggregate profile to act as the compliance auditor
        var reviewer = new Reviewer(Guid.NewGuid(), "Institutional Auditor");
        await AddAsync(reviewer);

        // Prepare the submission command payload targeting the newly generated company profile
        var submitCommand = new SubmitProviderApplicationCommand
        {
            ProviderId = providerId,
            CompanyDetails = "EdTech Solutions Corp Official Registration Metadata Dossier",
            VerificationDocumentsUrl = "https://documents.academicgateway.com/edtech-verify.pdf"
        };

        // ==========================================
        // 2. ACT (STAGE A) - SUBMIT COMPLIANCE DOSSIER
        // ==========================================
        Guid applicationId = await SendAsync(submitCommand);

        // Assert intermediate state: Application record is initialized and waiting in the pending review pool
        applicationId.Should().NotBeEmpty();
        var intermediateAppRecord = await FindAsync<ProviderApplication>(applicationId);
        intermediateAppRecord.Should().NotBeNull();
        intermediateAppRecord!.Status.Should().Be(ProviderApplicationStatus.PendingReview);

        // Build the evaluation decision command to approve the compliance request
        var reviewCommand = new ReviewProviderApplicationCommand
        {
            ApplicationId = applicationId,
            ReviewerId = reviewer.Id,
            IsApproved = true,
            RejectionReason = null
        };

        // ==========================================
        // 3. ACT (STAGE B) - AUDIT & CERTIFY PROFILE
        // ==========================================
        await SendAsync(reviewCommand);

        // ==========================================
        // 4. ASSERT FINAL INTEGRATED STATE
        // ==========================================

        // Verify the onboarding application record successfully transitioned to a terminal Approved status
        var updatedAppRecord = await FindAsync<ProviderApplication>(applicationId);
        updatedAppRecord.Should().NotBeNull();
        updatedAppRecord!.Status.Should().Be(ProviderApplicationStatus.Approved);
        updatedAppRecord.ReviewedById.Should().Be(reviewer.Id);

        // Verify cross-aggregate event handlers triggered to elevate the core partner verification standing
        var verifiedProviderProfile = await FindAsync<Provider>(providerId);
        verifiedProviderProfile.Should().NotBeNull();
        verifiedProviderProfile!.IsVerified.Should().BeTrue();
    }
}
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
/// End-to-end workflow integration tests validating partner onboarding rejection paths,
/// administrative evaluations, and preservation of the unverified profile state.
/// </summary>
[Collection("SharedDatabase")]
public class ProviderRejectionWorkflowTests : BaseIntegrationTest
{
    public ProviderRejectionWorkflowTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Validates the negative end-to-end workflow scenario where an unverified company profile submits compliance 
    /// paperwork, but an institutional auditor rejects it due to illegible or insufficient details. 
    /// The application status must reflect the rejection and the provider must remain unverified.
    /// </summary>
    [Fact]
    public async Task Should_RejectApplicationAndLeaveProviderUnverified_WhenReviewerRejects()
    {
        // ==========================================
        // 1. ARRANGE & INITIALIZE WORKFLOW
        // ==========================================

        // Register a corporate partner profile via the pipeline (natively initializes as unverified)
        var registerProviderCommand = new RegisterProviderCommand
        {
            Email = "workflow.rejection.prov@academicgateway.com",
            Username = "rejectionprov",
            Password = "SecurePassword123!",
            CompanyName = "Faulty Docs Corp",
            CompanyDescription = "Incomplete External Verification Entity",
            WebsiteUrl = "https://faulty-docs.com"
        };
        Guid providerId = await SendAsync(registerProviderCommand);

        // Instantiate an official faculty reviewer aggregate profile to act as the compliance auditor
        var reviewer = new Reviewer(Guid.NewGuid(), "Institutional Auditor");
        await AddAsync(reviewer);

        // Prepare the submission command payload targeting the newly generated company profile
        var submitCommand = new SubmitProviderApplicationCommand
        {
            ProviderId = providerId,
            CompanyDetails = "Faulty Docs Corp Registration Submission Folder",
            VerificationDocumentsUrl = "https://verify.com/bad-docs.pdf"
        };

        // Submit compliance dossier to transition into the pending pool
        Guid applicationId = await SendAsync(submitCommand);

        // Build the evaluation decision command to reject the compliance request with justification feedback
        const string rejectionReasonText = "Documents provided are completely illegible.";
        var rejectCommand = new ReviewProviderApplicationCommand
        {
            ApplicationId = applicationId,
            ReviewerId = reviewer.Id,
            IsApproved = false,
            RejectionReason = rejectionReasonText
        };

        // ==========================================
        // 2. ACT - EXECUTE COMPLIANCE REJECTION
        // ==========================================
        await SendAsync(rejectCommand);

        // ==========================================
        // 3. ASSERT FINAL INTEGRATED STATE
        // ==========================================

        // Verify the application aggregate successfully transitioned to a terminal Rejected status
        var application = await FindAsync<ProviderApplication>(applicationId);
        application.Should().NotBeNull();
        application!.Status.Should().Be(ProviderApplicationStatus.Rejected);
        application.RejectionReason.Should().Be(rejectionReasonText);

        // Verify the provider aggregate profile remained explicitly unverified
        var verifiedProvider = await FindAsync<Provider>(providerId);
        verifiedProvider.Should().NotBeNull();
        verifiedProvider!.IsVerified.Should().BeFalse();
    }
}
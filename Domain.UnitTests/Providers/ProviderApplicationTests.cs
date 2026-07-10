using System;
using System.Linq;
using AcademicGateway.Domain.Providers;
using AcademicGateway.Domain.Providers.Enums;
using AcademicGateway.Domain.Providers.Events;
using AcademicGateway.Domain.Providers.Exceptions;
using FluentAssertions;
using Xunit;

namespace AcademicGateway.Domain.UnitTests.Providers;

public class ProviderApplicationTests
{
    private readonly Guid _fallbackProviderId = Guid.NewGuid();
    private readonly string _fallbackDetails = "A prominent engineering organization focused on software infrastructure.";
    private readonly string _fallbackDocsUrl = "https://storage.gateway.local/verifications/docs_001.pdf";
    private readonly DateTime _fallbackCreatedTime = new(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);

    private ProviderApplication CreateValidTestInstance()
    {
        return new ProviderApplication(_fallbackProviderId, _fallbackDetails, _fallbackDocsUrl, _fallbackCreatedTime);
    }

    #region Constructor Tests

    [Fact]
    public void PrivateConstructor_ShouldInitializeDefaults_ForEfCoreHydration()
    {
        // Arrange & Act
        var application = (ProviderApplication)Activator.CreateInstance(typeof(ProviderApplication), true)!;

        // Assert
        application.Should().NotBeNull();
        application.Id.Should().Be(Guid.Empty);
        application.ProviderId.Should().Be(Guid.Empty);
        application.CompanyDetails.Should().BeEmpty();
        application.VerificationDocumentsUrl.Should().BeEmpty();
        application.Status.Should().Be(ProviderApplicationStatus.Draft);
        application.ReviewedById.Should().BeNull();
        application.RejectionReason.Should().BeNull();
        application.ReviewedAt.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldInitializeInDraftState_WhenParametersAreValid()
    {
        // Arrange
        var untrimmedDetails = "   " + _fallbackDetails + "   ";
        var untrimmedDocsUrl = "   " + _fallbackDocsUrl + "   ";

        // Act
        var application = new ProviderApplication(_fallbackProviderId, untrimmedDetails, untrimmedDocsUrl, _fallbackCreatedTime);

        // Assert
        application.Id.Should().NotBeEmpty();
        application.ProviderId.Should().Be(_fallbackProviderId);
        application.CompanyDetails.Should().Be(_fallbackDetails);
        application.VerificationDocumentsUrl.Should().Be(_fallbackDocsUrl);
        application.Status.Should().Be(ProviderApplicationStatus.Draft);
        application.CreatedAt.Should().Be(_fallbackCreatedTime);
        application.ReviewedById.Should().BeNull();
        application.ReviewedAt.Should().BeNull();
        application.RejectionReason.Should().BeNull();

        application.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProviderApplicationCreatedEvent>()
            .Which.Should().Match<ProviderApplicationCreatedEvent>(e =>
                e.ApplicationId == application.Id && e.ProviderId == _fallbackProviderId);
    }

    [Theory]
    [InlineData(true, "Details", "https://url.com", "Provider ID cannot be an empty Guid.")]
    [InlineData(false, null, "https://url.com", "Company details cannot be empty or whitespace.")]
    [InlineData(false, "", "https://url.com", "Company details cannot be empty or whitespace.")]
    [InlineData(false, "   ", "https://url.com", "Company details cannot be empty or whitespace.")]
    [InlineData(false, "Details", null, "Verification documents URL cannot be empty or whitespace.")]
    [InlineData(false, "Details", "", "Verification documents URL cannot be empty or whitespace.")]
    [InlineData(false, "Details", "   ", "Verification documents URL cannot be empty or whitespace.")]
    public void Constructor_ShouldThrowInvalidApplicationDetailsException_WhenRequiredFieldsAreInvalid(
        bool useEmptyProviderId, string? details, string? url, string expectedMessage)
    {
        // Arrange
        var providerId = useEmptyProviderId ? Guid.Empty : _fallbackProviderId;

        // Act
        Action act = () => _ = new ProviderApplication(providerId, details!, url!, _fallbackCreatedTime);

        // Assert
        act.Should().Throw<InvalidApplicationDetailsException>()
           .WithMessage(expectedMessage);
    }

    #endregion

    #region SubmitForReview Tests

    [Fact]
    public void SubmitForReview_ShouldTransitionToPendingReview_WhenCurrentStatusIsDraft()
    {
        // Arrange
        var application = CreateValidTestInstance();

        // Act
        application.SubmitForReview();

        // Assert
        application.Status.Should().Be(ProviderApplicationStatus.PendingReview);
        application.DomainEvents.Should().Contain(e => e is ProviderApplicationSubmittedEvent);
    }

    [Theory]
    [InlineData(ProviderApplicationStatus.PendingReview)]
    [InlineData(ProviderApplicationStatus.Approved)]
    [InlineData(ProviderApplicationStatus.Rejected)]
    public void SubmitForReview_ShouldThrowInvalidApplicationStatusException_WhenCurrentStatusIsNotDraft(ProviderApplicationStatus nonDraftStatus)
    {
        // Arrange
        var application = CreateValidTestInstance();

        // Mutate status via backdoor states using setup methods
        if (nonDraftStatus != ProviderApplicationStatus.Draft)
        {
            application.SubmitForReview(); // Now PendingReview
            if (nonDraftStatus == ProviderApplicationStatus.Approved)
            {
                application.Approve(Guid.NewGuid(), _fallbackCreatedTime.AddDays(1));
            }
            else if (nonDraftStatus == ProviderApplicationStatus.Rejected)
            {
                application.Reject(Guid.NewGuid(), "Declined parameters", _fallbackCreatedTime.AddDays(1));
            }
        }

        // Act
        Action act = () => application.SubmitForReview();

        // Assert
        act.Should().Throw<InvalidApplicationStatusException>();
    }

    #endregion

    #region Resubmit Tests

    [Fact]
    public void Resubmit_ShouldResetReviewTrackingAndTrimInputs_WhenCurrentStatusIsRejected()
    {
        // Arrange
        var application = CreateValidTestInstance();
        application.SubmitForReview();
        application.Reject(Guid.NewGuid(), "Incomplete compliance forms.", _fallbackCreatedTime.AddDays(1));

        var untrimmedDetails = "   Updated enterprise profiles and documents.   ";
        var untrimmedDocsUrl = "   https://storage.gateway.local/verifications/updated_docs.pdf   ";
        var expectedDetails = "Updated enterprise profiles and documents.";
        var expectedDocsUrl = "https://storage.gateway.local/verifications/updated_docs.pdf";

        // Act
        application.Resubmit(untrimmedDetails, untrimmedDocsUrl);

        // Assert
        application.Status.Should().Be(ProviderApplicationStatus.PendingReview);
        application.CompanyDetails.Should().Be(expectedDetails);
        application.VerificationDocumentsUrl.Should().Be(expectedDocsUrl);
        application.ReviewedById.Should().BeNull();
        application.ReviewedAt.Should().BeNull();
        application.RejectionReason.Should().BeNull();

        application.DomainEvents.Last().Should().BeOfType<ProviderApplicationResubmittedEvent>()
            .Which.Should().Match<ProviderApplicationResubmittedEvent>(e =>
                e.ApplicationId == application.Id && e.ProviderId == _fallbackProviderId);
    }

    [Theory]
    [InlineData(ProviderApplicationStatus.Draft)]
    [InlineData(ProviderApplicationStatus.PendingReview)]
    [InlineData(ProviderApplicationStatus.Approved)]
    public void Resubmit_ShouldThrowInvalidApplicationStatusException_WhenCurrentStatusIsNotRejected(ProviderApplicationStatus nonRejectedStatus)
    {
        // Arrange
        var application = CreateValidTestInstance();
        if (nonRejectedStatus == ProviderApplicationStatus.PendingReview)
        {
            application.SubmitForReview();
        }
        else if (nonRejectedStatus == ProviderApplicationStatus.Approved)
        {
            application.SubmitForReview();
            application.Approve(Guid.NewGuid(), _fallbackCreatedTime.AddDays(1));
        }

        // Act
        Action act = () => application.Resubmit("Details", "https://url.com");

        // Assert
        act.Should().Throw<InvalidApplicationStatusException>();
    }

    [Theory]
    [InlineData(null, "https://url.com")]
    [InlineData("", "https://url.com")]
    [InlineData("   ", "https://url.com")]
    [InlineData("Details", null)]
    [InlineData("Details", "")]
    [InlineData("Details", "   ")]
    public void Resubmit_ShouldThrowInvalidApplicationDetailsException_WhenUpdatedFieldsAreBlank(string? details, string? url)
    {
        // Arrange
        var application = CreateValidTestInstance();
        application.SubmitForReview();
        application.Reject(Guid.NewGuid(), "Rejection Reason", _fallbackCreatedTime.AddDays(1));

        // Act
        Action act = () => application.Resubmit(details!, url!);

        // Assert
        act.Should().Throw<InvalidApplicationDetailsException>();
    }

    #endregion

    #region Approve Tests

    [Fact]
    public void Approve_ShouldSetStatusToApprovedAndLogAuditData_WhenStatusIsPendingReview()
    {
        // Arrange
        var application = CreateValidTestInstance();
        application.SubmitForReview();
        var reviewerId = Guid.NewGuid();
        var approvalTime = _fallbackCreatedTime.AddDays(2);

        // Act
        application.Approve(reviewerId, approvalTime);

        // Assert
        application.Status.Should().Be(ProviderApplicationStatus.Approved);
        application.ReviewedById.Should().Be(reviewerId);
        application.ReviewedAt.Should().Be(approvalTime);
        application.RejectionReason.Should().BeNull();

        application.DomainEvents.Last().Should().BeOfType<ProviderApplicationApprovedEvent>()
            .Which.ProviderId.Should().Be(_fallbackProviderId);
    }

    [Fact]
    public void Approve_ShouldSucceed_WhenApprovalTimeIsExactlyEqualToCreationTime()
    {
        // Arrange
        var application = CreateValidTestInstance();
        application.SubmitForReview();
        var reviewerId = Guid.NewGuid();

        // Act
        application.Approve(reviewerId, _fallbackCreatedTime);

        // Assert
        application.Status.Should().Be(ProviderApplicationStatus.Approved);
        application.ReviewedAt.Should().Be(_fallbackCreatedTime);
    }

    [Fact]
    public void Approve_ShouldThrowInvalidApplicationStatusException_WhenStatusIsNotPendingReview()
    {
        // Arrange
        var application = CreateValidTestInstance(); // Draft state

        // Act
        Action act = () => application.Approve(Guid.NewGuid(), _fallbackCreatedTime.AddDays(1));

        // Assert
        act.Should().Throw<InvalidApplicationStatusException>();
    }

    [Fact]
    public void Approve_ShouldThrowInvalidApplicationDetailsException_WhenReviewerIdIsEmpty()
    {
        // Arrange
        var application = CreateValidTestInstance();
        application.SubmitForReview();

        // Act
        Action act = () => application.Approve(Guid.Empty, _fallbackCreatedTime.AddDays(1));

        // Assert
        act.Should().Throw<InvalidApplicationDetailsException>()
           .WithMessage("A valid reviewer ID must be provided to approve an application.");
    }

    [Fact]
    public void Approve_ShouldThrowInvalidApplicationDetailsException_WhenApprovalDatePrecedesCreationDate()
    {
        // Arrange
        var application = CreateValidTestInstance();
        application.SubmitForReview();
        var historicalDate = _fallbackCreatedTime.AddHours(-1);

        // Act
        Action act = () => application.Approve(Guid.NewGuid(), historicalDate);

        // Assert
        act.Should().Throw<InvalidApplicationDetailsException>()
           .WithMessage("Approval date cannot be older than the application creation date.");
    }

    #endregion

    #region Reject Tests

    [Fact]
    public void Reject_ShouldSetStatusToRejectedAndLogReason_WhenStatusIsPendingReview()
    {
        // Arrange
        var application = CreateValidTestInstance();
        application.SubmitForReview();
        var reviewerId = Guid.NewGuid();
        var reasonText = "   Invalid operational license key formats.   ";
        var rejectionTime = _fallbackCreatedTime.AddDays(2);

        // Act
        application.Reject(reviewerId, reasonText, rejectionTime);

        // Assert
        application.Status.Should().Be(ProviderApplicationStatus.Rejected);
        application.ReviewedById.Should().Be(reviewerId);
        application.ReviewedAt.Should().Be(rejectionTime);
        application.RejectionReason.Should().Be("Invalid operational license key formats.");

        application.DomainEvents.Last().Should().BeOfType<ProviderApplicationRejectedEvent>()
            .Which.Should().Match<ProviderApplicationRejectedEvent>(e =>
                e.ApplicationId == application.Id &&
                e.ProviderId == _fallbackProviderId &&
                e.ReviewerId == reviewerId &&
                e.Reason == "Invalid operational license key formats.");
    }

    [Fact]
    public void Reject_ShouldSucceed_WhenRejectionTimeIsExactlyEqualToCreationTime()
    {
        // Arrange
        var application = CreateValidTestInstance();
        application.SubmitForReview();
        var reviewerId = Guid.NewGuid();

        // Act
        application.Reject(reviewerId, "Valid Reason Mapping", _fallbackCreatedTime);

        // Assert
        application.Status.Should().Be(ProviderApplicationStatus.Rejected);
        application.ReviewedAt.Should().Be(_fallbackCreatedTime);
    }

    [Fact]
    public void Reject_ShouldThrowInvalidApplicationStatusException_WhenStatusIsNotPendingReview()
    {
        // Arrange
        var application = CreateValidTestInstance(); // Draft state

        // Act
        Action act = () => application.Reject(Guid.NewGuid(), "Reason", _fallbackCreatedTime.AddDays(1));

        // Assert
        act.Should().Throw<InvalidApplicationStatusException>();
    }

    [Theory]
    [InlineData(true, "Reason value")]
    [InlineData(false, null)]
    [InlineData(false, "")]
    [InlineData(false, "   ")]
    public void Reject_ShouldThrowInvalidApplicationDetailsException_WhenReviewerIdIsEmptyOrReasonIsBlank(bool useEmptyReviewerId, string? invalidReason)
    {
        // Arrange
        var application = CreateValidTestInstance();
        application.SubmitForReview();
        var reviewerId = useEmptyReviewerId ? Guid.Empty : Guid.NewGuid();

        // Act
        Action act = () => application.Reject(reviewerId, invalidReason!, _fallbackCreatedTime.AddDays(1));

        // Assert
        act.Should().Throw<InvalidApplicationDetailsException>();
    }

    [Fact]
    public void Reject_ShouldThrowInvalidApplicationDetailsException_WhenRejectionDatePrecedesCreationDate()
    {
        // Arrange
        var application = CreateValidTestInstance();
        application.SubmitForReview();
        var historicalDate = _fallbackCreatedTime.AddHours(-1);

        // Act
        Action act = () => application.Reject(Guid.NewGuid(), "Valid reason descriptive text", historicalDate);

        // Assert
        act.Should().Throw<InvalidApplicationDetailsException>()
           .WithMessage("Rejection date cannot be older than the application creation date.");
    }

    #endregion
}
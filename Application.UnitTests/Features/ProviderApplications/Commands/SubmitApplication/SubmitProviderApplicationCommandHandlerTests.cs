using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProviderApplications.Commands.SubmitProviderApplication;
using AcademicGateway.Domain.Providers;
using AcademicGateway.Domain.Providers.Enums;
using AcademicGateway.Domain.Providers.Exceptions;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.ProviderApplications.Commands.SubmitProviderApplication;

/// <summary>
/// Contains isolated unit verification routines for the <see cref="SubmitProviderApplicationCommandHandler"/>.
/// Validates 1-to-1 aggregate root allocation constraints, multi-path lifecycle transitions, resubmission state loops,
/// defensive fallback catch blocks, and pass-through domain aggregate detail invariants.
/// </summary>
public class SubmitProviderApplicationCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IDateTimeProvider> _mockDateTimeProvider;
    private readonly SubmitProviderApplicationCommandHandler _handler;
    private readonly DateTime _fixedUtcTime;

    /// <summary>
    /// Initializes a pristine instance of the test class with isolated database and calendar mock configurations.
    /// </summary>
    public SubmitProviderApplicationCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockDateTimeProvider = new Mock<IDateTimeProvider>();

        // Establish a stable chronological baseline for deterministic state comparison checks
        _fixedUtcTime = new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc);
        _mockDateTimeProvider.Setup(x => x.UtcNow).Returns(_fixedUtcTime);

        _handler = new SubmitProviderApplicationCommandHandler(_mockContext.Object, _mockDateTimeProvider.Object);
    }

    /// <summary>
    /// Assures that an unverified partner company with no previous application entries successfully generates
    /// a new onboarding record transitioned from internal Draft straight into the review evaluation pool.
    /// </summary>
    [Fact]
    public async Task Handle_GivenValidUnverifiedProviderAndNoPriorApplication_ShouldCreateApplicationAndReturnId()
    {
        // Arrange
        var targetProviderId = Guid.NewGuid();
        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = targetProviderId,
            CompanyDetails = "A fully compliant educational partner looking to host technical skill tracks.",
            VerificationDocumentsUrl = "https://storage.academicgateway.net/docs/compliance_v1.pdf"
        };

        var provider = new Provider(targetProviderId, "Corporate Tech Innovations");

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.ProviderApplications).Returns(new List<ProviderApplication>().BuildMockDbSet().Object);

        // Act
        var resultId = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        resultId.Should().NotBeEmpty();

        _mockContext.Verify(c => c.ProviderApplications.Add(It.Is<ProviderApplication>(a =>
            a.Id == resultId &&
            a.ProviderId == targetProviderId &&
            a.CompanyDetails == command.CompanyDetails &&
            a.VerificationDocumentsUrl == command.VerificationDocumentsUrl &&
            a.Status == ProviderApplicationStatus.PendingReview &&
            a.CreatedAt == _fixedUtcTime
        )), Times.Once);

        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that if a corporate partner's previous submission was formally Rejected, issuing a new command 
    /// triggers the stateful loop that updates the tracking fields and safely pushes it back into PendingReview.
    /// </summary>
    [Fact]
    public async Task Handle_GivenPriorApplicationIsRejected_ShouldTriggerResubmitDomainMethodAndReturnId()
    {
        // Arrange
        var targetProviderId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();
        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = targetProviderId,
            CompanyDetails = "Corrected corporate bio overview.",
            VerificationDocumentsUrl = "https://storage.net/fixed_credentials.pdf"
        };

        var provider = new Provider(targetProviderId, "Resubmitting Partner Co.");

        // Simulate a Rejected baseline status natively using domain methods
        var existingApplication = new ProviderApplication(targetProviderId, "Bad Details", "https://docs.com/blurry.pdf", _fixedUtcTime.AddDays(-5));
        existingApplication.SubmitForReview();
        existingApplication.Reject(reviewerId, "Filing details were blurry.", _fixedUtcTime.AddDays(-2));

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.ProviderApplications).Returns(new List<ProviderApplication> { existingApplication }.BuildMockDbSet().Object);

        // Act
        var resultId = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        resultId.Should().Be(existingApplication.Id);

        existingApplication.Status.Should().Be(ProviderApplicationStatus.PendingReview);
        existingApplication.CompanyDetails.Should().Be(command.CompanyDetails);
        existingApplication.VerificationDocumentsUrl.Should().Be(command.VerificationDocumentsUrl);

        // Assert historical audit fields are completely cleared out per workflow requirements
        existingApplication.ReviewedById.Should().BeNull();
        existingApplication.ReviewedAt.Should().BeNull();
        existingApplication.RejectionReason.Should().BeNull();

        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that if the executing provider profile target unique identifier cannot be mapped inside 
    /// the registry databases, execution stops instantly and a <see cref="KeyNotFoundException"/> is raised.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNonExistentProviderId_ShouldThrowKeyNotFoundExceptionAndAbort()
    {
        // Arrange
        var wrongProviderId = Guid.NewGuid();
        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = wrongProviderId,
            CompanyDetails = "Some Details",
            VerificationDocumentsUrl = "https://docs.com/doc.pdf"
        };

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider>().BuildMockDbSet().Object);
        _mockContext.Setup(c => c.ProviderApplications).Returns(new List<ProviderApplication>().BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*Provider profile with ID '{wrongProviderId}' was not found.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that attempting to issue a submission command while a previous onboarding record is already 
    /// standing active in an unalterable review pipeline status triggers an <see cref="InvalidApplicationStatusException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenActiveReviewApplicationAlreadyExists_ShouldThrowInvalidApplicationStatusException()
    {
        // Arrange
        var targetProviderId = Guid.NewGuid();
        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = targetProviderId,
            CompanyDetails = "Submitting duplicate application text to force compliance failure.",
            VerificationDocumentsUrl = "https://storage.net/doc.pdf"
        };

        var provider = new Provider(targetProviderId, "Spammer Solutions Corp");

        var existingApplication = new ProviderApplication(targetProviderId, "Original Details", "https://docs.com/original.pdf", _fixedUtcTime.AddDays(-2));
        existingApplication.SubmitForReview(); // Status = PendingReview

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.ProviderApplications).Returns(new List<ProviderApplication> { existingApplication }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidApplicationStatusException>();
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that attempting to issue a submission command when a previous application has achieved an Approved state 
    /// securely locks subsequent changes and throws an <see cref="InvalidApplicationStatusException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenApplicationIsAlreadyApproved_ShouldThrowInvalidApplicationStatusException()
    {
        // Arrange
        var targetProviderId = Guid.NewGuid();
        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = targetProviderId,
            CompanyDetails = "Submitting an alteration to a fully verified and approved record track.",
            VerificationDocumentsUrl = "https://storage.net/override.pdf"
        };

        var provider = new Provider(targetProviderId, "Verified Corporate Vendor");

        var existingApplication = new ProviderApplication(targetProviderId, "Original Details", "https://docs.com/original.pdf", _fixedUtcTime.AddDays(-5));
        existingApplication.SubmitForReview();
        existingApplication.Approve(Guid.NewGuid(), _fixedUtcTime.AddDays(-2)); // Status = Approved

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.ProviderApplications).Returns(new List<ProviderApplication> { existingApplication }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidApplicationStatusException>();
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if an application exists but resides in an unhandled pipeline state context (such as Draft),
    /// the handler's default defensive fallback path intercepts processing and throws an <see cref="InvalidApplicationStatusException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenExistingApplicationInDraftStatus_ShouldHitHandlerFallbackAndThrowInvalidApplicationStatusException()
    {
        // Arrange
        var targetProviderId = Guid.NewGuid();
        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = targetProviderId,
            CompanyDetails = "Attempting to push when a draft record already tracks inside database bounds.",
            VerificationDocumentsUrl = "https://storage.net/draft_overwrite.pdf"
        };

        var provider = new Provider(targetProviderId, "Draft Owner Corp");

        // Instantiate the entity but DO NOT invoke SubmitForReview(). Status remains 'Draft'.
        var existingApplication = new ProviderApplication(targetProviderId, "Draft Details Blocked", "https://docs.com/draft.pdf", _fixedUtcTime.AddDays(-1));

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.ProviderApplications).Returns(new List<ProviderApplication> { existingApplication }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidApplicationStatusException>();
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if invalid, empty, or whitespace text is provided for company descriptions during a new application,
    /// the core domain boundaries reject structural hydration, throwing an <see cref="InvalidApplicationDetailsException"/>.
    /// </summary>
    /// <remarks>
    /// Fixed xUnit1012 warning by altering the input variable type signature to a nullable string component.
    /// </remarks>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Handle_NewApplication_GivenInvalidCompanyDetails_ShouldPropagateInvalidApplicationDetailsException(string? invalidDetails)
    {
        // Arrange
        var targetProviderId = Guid.NewGuid();
        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = targetProviderId,
            CompanyDetails = invalidDetails!,
            VerificationDocumentsUrl = "https://storage.net/legal_docs.pdf"
        };

        var provider = new Provider(targetProviderId, "Corporate Entity Profile");

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.ProviderApplications).Returns(new List<ProviderApplication>().BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidApplicationDetailsException>()
            .WithMessage("*Company details cannot be empty or whitespace.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if an empty or whitespace URI reference is supplied for documentation locations during a new application,
    /// the aggregate invariants block generation by throwing an <see cref="InvalidApplicationDetailsException"/>.
    /// </summary>
    /// <remarks>
    /// Fixed xUnit1012 warning by altering the input variable type signature to a nullable string component.
    /// </remarks>
    [Theory]
    [InlineData("")]
    [InlineData(" \n \t ")]
    [InlineData(null)]
    public async Task Handle_NewApplication_GivenInvalidDocumentsUrl_ShouldPropagateInvalidApplicationDetailsException(string? invalidUrl)
    {
        // Arrange
        var targetProviderId = Guid.NewGuid();
        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = targetProviderId,
            CompanyDetails = "Valid textual description context detailing corporate operations baseline.",
            VerificationDocumentsUrl = invalidUrl!
        };

        var provider = new Provider(targetProviderId, "Corporate Entity Profile");

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.ProviderApplications).Returns(new List<ProviderApplication>().BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidApplicationDetailsException>()
            .WithMessage("*Verification documents URL cannot be empty or whitespace.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if an empty or whitespace string is passed for corporate backgrounds during a resubmission tracking loop,
    /// the domain aggregate root prevents updating, throwing an <see cref="InvalidApplicationDetailsException"/>.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_Resubmission_GivenInvalidCompanyDetails_ShouldPropagateInvalidApplicationDetailsException(string invalidDetails)
    {
        // Arrange
        var targetProviderId = Guid.NewGuid();
        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = targetProviderId,
            CompanyDetails = invalidDetails,
            VerificationDocumentsUrl = "https://storage.net/updated_files.pdf"
        };

        var provider = new Provider(targetProviderId, "Resubmitting Vendor");

        var existingApplication = new ProviderApplication(targetProviderId, "Original Details", "https://docs.com/old.pdf", _fixedUtcTime.AddDays(-5));
        existingApplication.SubmitForReview();
        existingApplication.Reject(Guid.NewGuid(), "Requires more thorough updates.", _fixedUtcTime.AddDays(-2));

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.ProviderApplications).Returns(new List<ProviderApplication> { existingApplication }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidApplicationDetailsException>()
            .WithMessage("*Company details cannot be empty or whitespace.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if an empty or whitespace string is passed for documentation files during a resubmission tracking loop,
    /// the domain aggregate root prevents updating, throwing an <see cref="InvalidApplicationDetailsException"/>.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" \t \n ")]
    public async Task Handle_Resubmission_GivenInvalidDocumentsUrl_ShouldPropagateInvalidApplicationDetailsException(string invalidUrl)
    {
        // Arrange
        var targetProviderId = Guid.NewGuid();
        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = targetProviderId,
            CompanyDetails = "Thorough and expanded corporate background portfolio details.",
            VerificationDocumentsUrl = invalidUrl
        };

        var provider = new Provider(targetProviderId, "Resubmitting Vendor");

        var existingApplication = new ProviderApplication(targetProviderId, "Original Details", "https://docs.com/old.pdf", _fixedUtcTime.AddDays(-5));
        existingApplication.SubmitForReview();
        existingApplication.Reject(Guid.NewGuid(), "Files were missing or corrupted.", _fixedUtcTime.AddDays(-2));

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.ProviderApplications).Returns(new List<ProviderApplication> { existingApplication }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidApplicationDetailsException>()
            .WithMessage("*Verification documents URL cannot be empty or whitespace.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
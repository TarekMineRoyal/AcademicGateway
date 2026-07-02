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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.ProviderApplications.Commands.SubmitProviderApplication;

/// <summary>
/// Contains isolated unit verification routines for the <see cref="SubmitProviderApplicationCommandHandler"/>.
/// Validates 1-to-1 aggregate root allocation constraints, multi-path lifecycle transitions, and resubmission state loops.
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

        // Instantiate core prerequisite dependencies via domain public constructors
        var provider = new Provider(targetProviderId, "Corporate Tech Innovations");

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.ProviderApplications).Returns(new List<ProviderApplication>().BuildMockDbSet().Object);

        // Act
        // Best Practice (xUnit1051): Pass TestContext.Current.CancellationToken for responsive test abort controls.
        var resultId = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        resultId.Should().NotBeEmpty();

        // Verify that the new aggregate root was fully populated and appended to the tracking table dataset
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

        // Establish an existing application and push it to an active review state natively
        var existingApplication = new ProviderApplication(targetProviderId, "Original Details", "https://docs.com/original.pdf", _fixedUtcTime.AddDays(-2));
        existingApplication.SubmitForReview();

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.ProviderApplications).Returns(new List<ProviderApplication> { existingApplication }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidApplicationStatusException>();
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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
        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = targetProviderId,
            CompanyDetails = "Corrected corporate bio overview.",
            VerificationDocumentsUrl = "https://storage.net/fixed_credentials.pdf"
        };

        var provider = new Provider(targetProviderId, "Resubmitting Partner Co.");

        // Simulate a Rejected baseline status natively via domain operations
        var existingApplication = new ProviderApplication(targetProviderId, "Bad Details", "https://docs.com/blurry.pdf", _fixedUtcTime.AddDays(-5));
        existingApplication.SubmitForReview();
        existingApplication.Reject(Guid.NewGuid(), "Filing details were blurry.", _fixedUtcTime.AddDays(-2));

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.ProviderApplications).Returns(new List<ProviderApplication> { existingApplication }.BuildMockDbSet().Object);

        // Act
        var resultId = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        resultId.Should().Be(existingApplication.Id);

        // Verify state mutation fields were accurately reset and pushed back to the evaluation grid
        existingApplication.Status.Should().Be(ProviderApplicationStatus.PendingReview);
        existingApplication.CompanyDetails.Should().Be(command.CompanyDetails);
        existingApplication.VerificationDocumentsUrl.Should().Be(command.VerificationDocumentsUrl);
        existingApplication.RejectionReason.Should().BeNull();

        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }
}
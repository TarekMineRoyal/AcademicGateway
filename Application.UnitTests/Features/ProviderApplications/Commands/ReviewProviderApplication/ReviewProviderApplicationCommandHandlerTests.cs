using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProviderApplications.Commands.ReviewProviderApplication;
using AcademicGateway.Domain.Providers;
using AcademicGateway.Domain.Providers.Enums;
using AcademicGateway.Domain.Reviewers;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.ProviderApplications.Commands.ReviewProviderApplication;

/// <summary>
/// Contains isolated unit verification routines for the <see cref="ReviewProviderApplicationCommandHandler"/>.
/// Validates review workflow state transitions, audit profile matches, feedback logging, and transactional checks.
/// </summary>
public class ReviewProviderApplicationCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IDateTimeProvider> _mockDateTimeProvider;
    private readonly ReviewProviderApplicationCommandHandler _handler;
    private readonly DateTime _fixedUtcTime;

    /// <summary>
    /// Initializes a pristine instance of the test suite, setting up structured mock contexts.
    /// </summary>
    public ReviewProviderApplicationCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockDateTimeProvider = new Mock<IDateTimeProvider>();

        // Establish a stable chronological base for rule validation
        _fixedUtcTime = new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc);
        _mockDateTimeProvider.Setup(x => x.UtcNow).Returns(_fixedUtcTime);

        _handler = new ReviewProviderApplicationCommandHandler(_mockContext.Object, _mockDateTimeProvider.Object);
    }

    /// <summary>
    /// Assures that a positive compliance evaluation decision advances a pending enrollment record 
    /// to an Approved status while attaching accurate reviewer timestamps and tracking IDs.
    /// </summary>
    [Fact]
    public async Task Handle_GivenValidApprovalCommand_ShouldTransitionStatusToApprovedAndLogSignatures()
    {
        // Arrange
        var targetApplicationId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();

        var command = new ReviewProviderApplicationCommand
        {
            ApplicationId = targetApplicationId,
            ReviewerId = reviewerId,
            IsApproved = true,
            RejectionReason = null
        };

        // Best Practice: Instantiate domain objects using official constructor and behavior methods
        var application = new ProviderApplication(providerId, "FinTech Software firm overview.", "https://docs.com/verify.pdf", _fixedUtcTime.AddDays(-2));

        // Precondition Check: State machine must explicitly stand at PendingReview to process decisions
        application.SubmitForReview();

        // Securely inject the generated ID since tracking primary keys are private-set by the DB mapping layer
        SetPrivateProperty(application, nameof(ProviderApplication.Id), targetApplicationId);

        var reviewer = new Reviewer(reviewerId, "Administrator Sarah Jenkins");

        _mockContext.Setup(c => c.ProviderApplications).Returns(new List<ProviderApplication> { application }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.Reviewers).Returns(new List<Reviewer> { reviewer }.BuildMockDbSet().Object);

        // Act
        // Best Practice (xUnit1051): Supply TestContext.Current.CancellationToken for responsive abort controls.
        await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        application.Status.Should().Be(ProviderApplicationStatus.Approved);
        application.ReviewedById.Should().Be(reviewerId);
        application.ReviewedAt.Should().Be(_fixedUtcTime);
        application.RejectionReason.Should().BeNull();

        // Note: Actual Provider.IsVerified flag adjustments execute inside the AppDbContext SaveChanges outbox interceptor,
        // which falls under integration testing bounds. Here we verify the handler pushes the atomic transactional save.
        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that a negative compliance decision transitions the application state to Rejected, 
    /// signs the reviewer identity, and imprints the audit justification context text.
    /// </summary>
    [Fact]
    public async Task Handle_GivenValidRejectionCommand_ShouldTransitionStatusToRejectedAndLogFeedback()
    {
        // Arrange
        var targetApplicationId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();
        var standardRejectionReason = "The submitted corporate filing documents are blurry and unreadable.";

        var command = new ReviewProviderApplicationCommand
        {
            ApplicationId = targetApplicationId,
            ReviewerId = reviewerId,
            IsApproved = false,
            RejectionReason = standardRejectionReason
        };

        var application = new ProviderApplication(providerId, "Corporate Profile Spec", "https://docs.com/bad.pdf", _fixedUtcTime.AddDays(-5));
        application.SubmitForReview();
        SetPrivateProperty(application, nameof(ProviderApplication.Id), targetApplicationId);

        var reviewer = new Reviewer(reviewerId, "Compliance Officer Jenkins");

        _mockContext.Setup(c => c.ProviderApplications).Returns(new List<ProviderApplication> { application }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.Reviewers).Returns(new List<Reviewer> { reviewer }.BuildMockDbSet().Object);

        // Act
        await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        application.Status.Should().Be(ProviderApplicationStatus.Rejected);
        application.ReviewedById.Should().Be(reviewerId);
        application.ReviewedAt.Should().Be(_fixedUtcTime);
        application.RejectionReason.Should().Be(standardRejectionReason);

        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that evaluating a missing or invalid application tracking identifier context 
    /// securely breaks execution and throws a precise <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNonExistentApplicationId_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var wrongApplicationId = Guid.NewGuid();
        var command = new ReviewProviderApplicationCommand
        {
            ApplicationId = wrongApplicationId,
            ReviewerId = Guid.NewGuid(),
            IsApproved = true
        };

        _mockContext.Setup(c => c.ProviderApplications).Returns(new List<ProviderApplication>().BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*Provider application with ID '{wrongApplicationId}' was not found.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if the auditor profile unique identification context is missing from the directory, 
    /// a <see cref="KeyNotFoundException"/> is thrown and data mutations are safely blocked.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNonExistentReviewerId_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var targetApplicationId = Guid.NewGuid();
        var wrongReviewerId = Guid.NewGuid();

        var command = new ReviewProviderApplicationCommand
        {
            ApplicationId = targetApplicationId,
            ReviewerId = wrongReviewerId,
            IsApproved = true
        };

        var application = new ProviderApplication(Guid.NewGuid(), "Company Details", "https://docs.com/files.pdf", _fixedUtcTime);
        SetPrivateProperty(application, nameof(ProviderApplication.Id), targetApplicationId);

        _mockContext.Setup(c => c.ProviderApplications).Returns(new List<ProviderApplication> { application }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.Reviewers).Returns(new List<Reviewer>().BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*Reviewer domain profile with ID '{wrongReviewerId}' was not found within the audit directory.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static void SetPrivateProperty(object obj, string propertyName, object value) =>
        obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?
           .SetValue(obj, value);
}
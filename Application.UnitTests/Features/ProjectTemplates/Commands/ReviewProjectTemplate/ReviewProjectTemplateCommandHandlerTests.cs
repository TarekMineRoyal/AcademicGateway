using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProjectTemplates.Commands.ReviewProjectTemplate;
using AcademicGateway.Domain.ProjectTemplates;
using AcademicGateway.Domain.ProjectTemplates.Enums;
using AcademicGateway.Domain.ProjectTemplates.Exceptions;
using AcademicGateway.Domain.Reviewers;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.ProjectTemplates.Commands.ReviewProjectTemplate;

/// <summary>
/// Contains isolated unit verification routines for the <see cref="ReviewProjectTemplateCommandHandler"/>.
/// Validates review workflow state transitions, audit profile validation checks, conditional null-coalescing loops, 
/// and strict domain aggregate lifecycle constraints.
/// </summary>
public class ReviewProjectTemplateCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly ReviewProjectTemplateCommandHandler _handler;

    /// <summary>
    /// Initializes a pristine instance of the test class with isolated database mock mappings.
    /// </summary>
    public ReviewProjectTemplateCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new ReviewProjectTemplateCommandHandler(_mockContext.Object);
    }

    /// <summary>
    /// Assures that processing an authorized approval request correctly moves a pending template 
    /// blueprint to an Approved status and logs the transaction.
    /// </summary>
    [Fact]
    public async Task Handle_GivenValidApprovalCommand_ShouldTransitionStatusToApproved()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();

        var template = new ProjectTemplate(
            title: "Data Engineering Basics",
            description: "A comprehensive introductory track mapping data engineering principles and pipelines.",
            providerId: providerId);

        // Precondition: Shift the aggregate root state machine out of Draft into PendingReview
        template.SubmitForReview();

        var reviewer = new Reviewer(reviewerId, "Dr. Sarah Jenkins");

        var command = new ReviewProjectTemplateCommand
        {
            TemplateId = template.Id,
            ReviewerId = reviewerId,
            IsApproved = true,
            RejectionReason = null
        };

        _mockContext.Setup(c => c.ProjectTemplates).Returns(new List<ProjectTemplate> { template }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.Reviewers).Returns(new List<Reviewer> { reviewer }.BuildMockDbSet().Object);

        // Act
        await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        template.Status.Should().Be(ProjectTemplateStatus.Approved);
        template.ReviewerFeedback.Should().BeNull();

        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that a negative audit decision transitions the project template lifecycle state to 
    /// Rejected and securely logs the auditor's administrative justification payload.
    /// </summary>
    [Fact]
    public async Task Handle_GivenValidRejectionCommand_ShouldTransitionStatusToRejectedAndLogReason()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();
        var expectedRejectionNotes = "The curriculum scope is too broad for an introductory alignment tier.";

        var template = new ProjectTemplate(
            title: "Data Engineering Basics",
            description: "A comprehensive introductory track mapping data engineering principles and pipelines.",
            providerId: providerId);

        template.SubmitForReview();

        var reviewer = new Reviewer(reviewerId, "Dr. Sarah Jenkins");

        var command = new ReviewProjectTemplateCommand
        {
            TemplateId = template.Id,
            ReviewerId = reviewerId,
            IsApproved = false,
            RejectionReason = expectedRejectionNotes
        };

        _mockContext.Setup(c => c.ProjectTemplates).Returns(new List<ProjectTemplate> { template }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.Reviewers).Returns(new List<Reviewer> { reviewer }.BuildMockDbSet().Object);

        // Act
        await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        template.Status.Should().Be(ProjectTemplateStatus.Rejected);
        template.ReviewerFeedback.Should().Be(expectedRejectionNotes);

        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that when a rejection command contains a null reason argument, the handler's
    /// null-coalescing guard safely intercepts the context and imprints the default fallback string.
    /// </summary>
    [Fact]
    public async Task Handle_GivenRejectionWithNullReason_ShouldUseDefaultFallbackString()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();
        const string expectedFallback = "No specific rejection details provided by reviewer.";

        var template = new ProjectTemplate(
            title: "Advanced Cloud Deployments",
            description: "Comprehensive blueprint tracking multi-zone infrastructure routing profiles.",
            providerId: providerId);

        template.SubmitForReview();

        var reviewer = new Reviewer(reviewerId, "Dr. Sarah Jenkins");

        var command = new ReviewProjectTemplateCommand
        {
            TemplateId = template.Id,
            ReviewerId = reviewerId,
            IsApproved = false,
            RejectionReason = null // Explicitly triggers the null coalescing fallback pathway
        };

        _mockContext.Setup(c => c.ProjectTemplates).Returns(new List<ProjectTemplate> { template }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.Reviewers).Returns(new List<Reviewer> { reviewer }.BuildMockDbSet().Object);

        // Act
        await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        template.Status.Should().Be(ProjectTemplateStatus.Rejected);
        template.ReviewerFeedback.Should().Be(expectedFallback);

        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that when a rejection command supplies a whitespace or empty justification string,
    /// the context passes through the handler loop and triggers the aggregate domain invariant protection guard.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("     ")]
    public async Task Handle_GivenRejectionWithWhitespaceReason_ShouldPropagateInvalidTemplateDetailsExceptionAndAbort(string whitespaceReason)
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();

        var template = new ProjectTemplate(
            title: "Advanced Cloud Deployments",
            description: "Comprehensive blueprint tracking multi-zone infrastructure routing profiles.",
            providerId: providerId);

        template.SubmitForReview();

        var reviewer = new Reviewer(reviewerId, "Dr. Sarah Jenkins");

        var command = new ReviewProjectTemplateCommand
        {
            TemplateId = template.Id,
            ReviewerId = reviewerId,
            IsApproved = false,
            RejectionReason = whitespaceReason
        };

        _mockContext.Setup(c => c.ProjectTemplates).Returns(new List<ProjectTemplate> { template }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.Reviewers).Returns(new List<Reviewer> { reviewer }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidTemplateDetailsException>()
            .WithMessage("*A strict justification reason must be logged for permanent rejection.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that performing a review action on a missing or invalid template identification tracker 
    /// halts processing execution and throws a precise <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNonExistentTemplateId_ShouldThrowKeyNotFoundExceptionAndAbort()
    {
        // Arrange
        var wrongTemplateId = Guid.NewGuid();
        var command = new ReviewProjectTemplateCommand
        {
            TemplateId = wrongTemplateId,
            ReviewerId = Guid.NewGuid(),
            IsApproved = true
        };

        _mockContext.Setup(c => c.ProjectTemplates).Returns(new List<ProjectTemplate>().BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*Project template with ID '{wrongTemplateId}' was not found.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if the auditor profile identification context is missing from the directory registers,
    /// a <see cref="KeyNotFoundException"/> is thrown and data transactions are safely blocked.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNonExistentReviewerId_ShouldThrowKeyNotFoundExceptionAndAbort()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var wrongReviewerId = Guid.NewGuid();

        var template = new ProjectTemplate(
            title: "Title Structure Basics",
            description: "Standard validation compliant placeholder summary data.",
            providerId: providerId);

        var command = new ReviewProjectTemplateCommand
        {
            TemplateId = template.Id,
            ReviewerId = wrongReviewerId,
            IsApproved = true
        };

        _mockContext.Setup(c => c.ProjectTemplates).Returns(new List<ProjectTemplate> { template }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.Reviewers).Returns(new List<Reviewer>().BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*Reviewer domain profile with ID '{wrongReviewerId}' was not found within the audit directory.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that attempting to execute an approval state change when the underlying aggregate is not
    /// in PendingReview status breaks workflow constraints and throws an <see cref="InvalidTemplateStatusException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenTemplateInDraftStatus_WhenApproving_ShouldPropagateInvalidTemplateStatusException()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();

        var template = new ProjectTemplate(
            title: "Title Structure Basics",
            description: "Standard validation compliant placeholder summary data.",
            providerId: providerId);

        // Precondition violation: Left in default 'Draft' state, bypassing SubmitForReview()

        var reviewer = new Reviewer(reviewerId, "Auditor Jenkins");

        var command = new ReviewProjectTemplateCommand
        {
            TemplateId = template.Id,
            ReviewerId = reviewerId,
            IsApproved = true
        };

        _mockContext.Setup(c => c.ProjectTemplates).Returns(new List<ProjectTemplate> { template }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.Reviewers).Returns(new List<Reviewer> { reviewer }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidTemplateStatusException>();
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that attempting to execute a permanent rejection state mutation when the aggregate has already achieved
    /// an Approved status breaks state sequence rules and throws an <see cref="InvalidTemplateStatusException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenTemplateAlreadyApproved_WhenRejecting_ShouldPropagateInvalidTemplateStatusException()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();

        var template = new ProjectTemplate(
            title: "Title Structure Basics",
            description: "Standard validation compliant placeholder summary data.",
            providerId: providerId);

        template.SubmitForReview();
        template.Approve(); // Precondition violation: Shifted out of review boundaries entirely

        var reviewer = new Reviewer(reviewerId, "Auditor Jenkins");

        var command = new ReviewProjectTemplateCommand
        {
            TemplateId = template.Id,
            ReviewerId = reviewerId,
            IsApproved = false,
            RejectionReason = "Late administrative rejection attempt."
        };

        _mockContext.Setup(c => c.ProjectTemplates).Returns(new List<ProjectTemplate> { template }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.Reviewers).Returns(new List<Reviewer> { reviewer }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidTemplateStatusException>();
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
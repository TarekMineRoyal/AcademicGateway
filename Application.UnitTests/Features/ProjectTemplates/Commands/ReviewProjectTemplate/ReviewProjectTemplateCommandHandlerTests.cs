using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProjectTemplates.Commands.ReviewProjectTemplate;
using AcademicGateway.Domain.ProjectTemplates;
using AcademicGateway.Domain.ProjectTemplates.Enums;
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
/// Validates review workflow state transitions, audit profile checks, and final approval/rejection outcomes.
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

        // Best Practice: Instantiate domain aggregates via standard constructors to respect encapsulations
        var template = new ProjectTemplate(
            title: "Data Engineering Basics",
            description: "A comprehensive introductory track mapping data engineering principles and pipelines.",
            providerId: providerId);

        // Precondition: Shift the aggregate root state machine out of Draft into PendingReview
        template.SubmitForReview();

        var reviewer = new Reviewer(reviewerId, "Dr. Sarah Jenkins");

        var command = new ReviewProjectTemplateCommand
        {
            TemplateId = template.Id, // Link directly to the aggregate-generated unique identity code
            ReviewerId = reviewerId,
            IsApproved = true,
            RejectionReason = null
        };

        _mockContext.Setup(c => c.ProjectTemplates).Returns(new List<ProjectTemplate> { template }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.Reviewers).Returns(new List<Reviewer> { reviewer }.BuildMockDbSet().Object);

        // Act
        // Best Practice (xUnit1051): Use TestContext.Current.CancellationToken for responsive abort controls.
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
    /// Assures that performing a review actions on a missing or invalid template identification tracker 
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
}
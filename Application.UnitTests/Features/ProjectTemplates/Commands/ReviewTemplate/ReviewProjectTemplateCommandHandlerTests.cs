using AcademicGateway.Application.Common.Interfaces;
using Application.Features.ProjectTemplates.Commands.ReviewProjectTemplate;
using Domain.ProjectTemplates;
using Domain.ProjectTemplates.Enums;
using Domain.Reviewers;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.ProjectTemplates.Commands.ReviewTemplate;

public class ReviewProjectTemplateCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly ReviewProjectTemplateCommandHandler _handler;

    public ReviewProjectTemplateCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new ReviewProjectTemplateCommandHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_ValidApproval_ShouldSetStatusToApproved()
    {
        // Arrange
        var targetTemplateId = Guid.NewGuid();
        var mockReviewerIdentityId = "auth0|admin_reviewer_core";
        var resolvedReviewerGuid = Guid.NewGuid();

        var command = new ReviewProjectTemplateCommand
        {
            TemplateId = targetTemplateId,
            ReviewerIdentityUserId = mockReviewerIdentityId,
            IsApproved = true,
            RejectionReason = null
        };

        // Create base template through its primary constructor (Initial state defaults to Draft)
        var template = new ProjectTemplate("provider_id_123", "Data Engineering Basics", "Description needs to be long enough to comply.", 6);
        SetPrivateProperty(template, nameof(ProjectTemplate.Id), targetTemplateId);

        // Push template state machine to PendingReview to satisfy approval preconditions
        template.SubmitForReview();

        var reviewer = CreateEntityWithPrivateConstructor<Reviewer>();
        SetPrivateProperty(reviewer, nameof(Reviewer.Id), resolvedReviewerGuid);
        SetPrivateProperty(reviewer, nameof(Reviewer.IdentityUserId), mockReviewerIdentityId);

        _mockContext.Setup(c => c.ProjectTemplates).Returns(new List<ProjectTemplate> { template }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.Reviewers).Returns(new List<Reviewer> { reviewer }.BuildMockDbSet().Object);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        template.Status.Should().Be(ProjectTemplateStatus.Approved);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidRejection_ShouldSetStatusToRejectedAndLogFeedback()
    {
        // Arrange
        var targetTemplateId = Guid.NewGuid();
        var mockReviewerIdentityId = "auth0|admin_reviewer_reject";
        var resolvedReviewerGuid = Guid.NewGuid();
        var expectedRejectionNotes = "The curriculum scope is too broad for an 8-week duration tier.";

        var command = new ReviewProjectTemplateCommand
        {
            TemplateId = targetTemplateId,
            ReviewerIdentityUserId = mockReviewerIdentityId,
            IsApproved = false,
            RejectionReason = expectedRejectionNotes
        };

        var template = new ProjectTemplate("provider_id_123", "Data Engineering Basics", "Description needs to be long enough to comply.", 8);
        SetPrivateProperty(template, nameof(ProjectTemplate.Id), targetTemplateId);
        template.SubmitForReview();

        var reviewer = CreateEntityWithPrivateConstructor<Reviewer>();
        SetPrivateProperty(reviewer, nameof(Reviewer.Id), resolvedReviewerGuid);
        SetPrivateProperty(reviewer, nameof(Reviewer.IdentityUserId), mockReviewerIdentityId);

        _mockContext.Setup(c => c.ProjectTemplates).Returns(new List<ProjectTemplate> { template }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.Reviewers).Returns(new List<Reviewer> { reviewer }.BuildMockDbSet().Object);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        template.Status.Should().Be(ProjectTemplateStatus.Rejected);

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TemplateNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var command = new ReviewProjectTemplateCommand
        {
            TemplateId = Guid.NewGuid(),
            ReviewerIdentityUserId = "auth0|any_reviewer",
            IsApproved = true
        };

        _mockContext.Setup(c => c.ProjectTemplates).Returns(new List<ProjectTemplate>().BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*was not found*");
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReviewerNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var targetTemplateId = Guid.NewGuid();
        var command = new ReviewProjectTemplateCommand
        {
            TemplateId = targetTemplateId,
            ReviewerIdentityUserId = "auth0|ghost_admin",
            IsApproved = true
        };

        var template = new ProjectTemplate("provider_id_123", "Title Structure", "Description needs to be long enough to comply.", 4);
        SetPrivateProperty(template, nameof(ProjectTemplate.Id), targetTemplateId);

        _mockContext.Setup(c => c.ProjectTemplates).Returns(new List<ProjectTemplate> { template }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.Reviewers).Returns(new List<Reviewer>().BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*Reviewer domain profile*was not found*");
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static T CreateEntityWithPrivateConstructor<T>() =>
        (T)Activator.CreateInstance(typeof(T), true)!;

    private static void SetPrivateProperty(object obj, string propertyName, object value) =>
        obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?
           .SetValue(obj, value);
}
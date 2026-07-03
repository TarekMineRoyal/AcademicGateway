using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.SupervisionRequests.Commands.ReviewSupervisionRequest;
using AcademicGateway.Domain.ProjectInstances;
using AcademicGateway.Domain.ProjectInstances.Enums;
using AcademicGateway.Domain.ProjectInstances.Exceptions;
using FluentAssertions;
using MediatR;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.SupervisionRequests.Commands.ReviewSupervisionRequest;

/// <summary>
/// Contains completely isolated production-grade unit verification tests for the <see cref="ReviewSupervisionRequestCommandHandler"/>.
/// Validates repository graph filtering, security context role assertions, multi-tenant perimeter guards,
/// state machine domain invariant tracking, and atomic persistence framework outputs.
/// </summary>
public class ReviewSupervisionRequestCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IDateTimeProvider> _mockDateTimeProvider;
    private readonly ReviewSupervisionRequestCommandHandler _handler;
    private readonly DateTime _deterministicUtcNow;

    /// <summary>
    /// Initializes a pristine instance of the <see cref="ReviewSupervisionRequestCommandHandlerTests"/> class with isolated tracking mocks.
    /// </summary>
    public ReviewSupervisionRequestCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockDateTimeProvider = new Mock<IDateTimeProvider>();

        _handler = new ReviewSupervisionRequestCommandHandler(
            _mockContext.Object,
            _mockCurrentUserService.Object,
            _mockDateTimeProvider.Object);

        _deterministicUtcNow = new DateTime(2026, 7, 3, 14, 0, 0, DateTimeKind.Utc);
        _mockDateTimeProvider.Setup(d => d.UtcNow).Returns(_deterministicUtcNow);
    }

    /// <summary>
    /// Assures that when the correctly assigned academic faculty member evaluates an outstanding request with an approval choice, 
    /// the request transitions to Accepted, the parent instance links the supervisor key, and changes save atomically.
    /// </summary>
    [Fact]
    public async Task Handle_GivenValidCommandWithAcceptTrue_ShouldAcceptSupervisionRequestAndCommitRelationalChanges()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var targetRequestId = Guid.NewGuid();
        var assignedProfessorId = Guid.NewGuid();

        var command = new ReviewSupervisionRequestCommand
        {
            ProjectInstanceId = targetInstanceId,
            SupervisionRequestId = targetRequestId,
            Accept = true,
            RejectionReason = null
        };

        // Construct a workspace aggregate containing a pending supervision child item matching the targeted profesor key
        var projectInstance = CreateMockProjectInstance(Guid.NewGuid(), ProjectInstanceStatus.AwaitingSupervision, assignedProfessorId);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        var requestLog = projectInstance.SupervisionRequests.First();
        SetEntityPrivateProperty(requestLog, nameof(SupervisionRequest.Id), targetRequestId);
        SetEntityPrivateProperty(requestLog, nameof(SupervisionRequest.Status), SupervisionRequestStatus.Pending);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(assignedProfessorId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(Unit.Value);
        requestLog.Status.Should().Be(SupervisionRequestStatus.Accepted);
        projectInstance.SupervisorId.Should().Be(assignedProfessorId);
        projectInstance.Status.Should().Be(ProjectInstanceStatus.Active);

        // Confirm the correct notification event side-effects got appended safely
        projectInstance.DomainEvents.Should().Contain(e => e.GetType().Name == "SupervisionRequestAcceptedEvent");

        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that when the assigned professor declines an invitation, providing a valid textual justification reason string,
    /// the invitation status falls to Rejected, the reason is logged, and changes flush to relational tables cleanly.
    /// </summary>
    [Fact]
    public async Task Handle_GivenValidCommandWithAcceptFalse_ShouldRejectSupervisionRequestAndCommitRelationalChanges()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var targetRequestId = Guid.NewGuid();
        var assignedProfessorId = Guid.NewGuid();

        var command = new ReviewSupervisionRequestCommand
        {
            ProjectInstanceId = targetInstanceId,
            SupervisionRequestId = targetRequestId,
            Accept = false,
            RejectionReason = "Current laboratory research commitments match maximum allocation thresholds."
        };

        var projectInstance = CreateMockProjectInstance(Guid.NewGuid(), ProjectInstanceStatus.AwaitingSupervision, assignedProfessorId);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        var requestLog = projectInstance.SupervisionRequests.First();
        SetEntityPrivateProperty(requestLog, nameof(SupervisionRequest.Id), targetRequestId);
        SetEntityPrivateProperty(requestLog, nameof(SupervisionRequest.Status), SupervisionRequestStatus.Pending);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(assignedProfessorId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(Unit.Value);
        requestLog.Status.Should().Be(SupervisionRequestStatus.Rejected);
        requestLog.RejectionReason.Should().Be(command.RejectionReason);
        projectInstance.SupervisorId.Should().BeNull();

        // Confirm side-effect rejection metrics domain events got appended
        projectInstance.DomainEvents.Should().Contain(e => e.GetType().Name == "SupervisionRequestRejectedEvent");

        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that looking up a non-existent project workspace tracking key isolates handler execution 
    /// lines cleanly and propagates a descriptive <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNonExistentProjectInstanceId_ShouldThrowKeyNotFoundExceptionAndAbortTransaction()
    {
        // Arrange
        var missingInstanceId = Guid.NewGuid();
        var command = new ReviewSupervisionRequestCommand
        {
            ProjectInstanceId = missingInstanceId,
            SupervisionRequestId = Guid.NewGuid(),
            Accept = true
        };

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance>().BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*The target project instance workspace with ID '{missingInstanceId}' was not found.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that looking up an invalid request identity tracking key that does not belong to the matching 
    /// project instance aggregate child collection raises a structured <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNonExistentSupervisionRequestId_ShouldThrowKeyNotFoundExceptionAndProtectTransactionState()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var missingRequestId = Guid.NewGuid();

        var command = new ReviewSupervisionRequestCommand
        {
            ProjectInstanceId = targetInstanceId,
            SupervisionRequestId = missingRequestId,
            Accept = true
        };

        var projectInstance = CreateMockProjectInstance(Guid.NewGuid(), ProjectInstanceStatus.AwaitingSupervision, Guid.NewGuid());
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*The requested academic supervision invite record with ID '{missingRequestId}' was not found within this project context.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if an unauthenticated user context profile tries to trigger a review choice over an open request, 
    /// perimeter security blocks the workflow instantly with an <see cref="UnauthorizedAccessException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenUnauthenticatedUserContext_ShouldThrowUnauthorizedAccessExceptionAndBlockExecution()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var targetRequestId = Guid.NewGuid();

        var command = new ReviewSupervisionRequestCommand
        {
            ProjectInstanceId = targetInstanceId,
            SupervisionRequestId = targetRequestId,
            Accept = true
        };

        var projectInstance = CreateMockProjectInstance(Guid.NewGuid(), ProjectInstanceStatus.AwaitingSupervision, Guid.NewGuid());
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        var requestLog = projectInstance.SupervisionRequests.First();
        SetEntityPrivateProperty(requestLog, nameof(SupervisionRequest.Id), targetRequestId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(false);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Access Denied: You are not authorized to evaluate this supervision invitation.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if a professor attempts to evaluate a request that was directed to a different professor user account,
    /// cross-tenant boundary protections halt the call and raise an <see cref="UnauthorizedAccessException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenAuthenticatedProfessorButNotTheTargetedRecipient_ShouldThrowUnauthorizedAccessExceptionAndProtectState()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var targetRequestId = Guid.NewGuid();
        var realRecipientProfessorId = Guid.NewGuid();
        var badIntruderProfessorId = Guid.NewGuid();

        var command = new ReviewSupervisionRequestCommand
        {
            ProjectInstanceId = targetInstanceId,
            SupervisionRequestId = targetRequestId,
            Accept = true
        };

        var projectInstance = CreateMockProjectInstance(Guid.NewGuid(), ProjectInstanceStatus.AwaitingSupervision, realRecipientProfessorId);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        var requestLog = projectInstance.SupervisionRequests.First();
        SetEntityPrivateProperty(requestLog, nameof(SupervisionRequest.Id), targetRequestId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(badIntruderProfessorId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Access Denied: You are not authorized to evaluate this supervision invitation.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that when declining a request without providing a rejection reason,
    /// the system defaults the message to "Declined" and successfully commits the change.
    /// </summary>
    [Theory]
    [InlineData("", "Empty Structural Content String Layout")]
    [InlineData("    ", "Whitespace Structural Content String Layout")]
    [InlineData(null, "Explicitly Null Structural Parameter Reference Layout")]
    public async Task Handle_GivenDeclineChoiceWithMissingRejectionReason_ShouldDefaultToDeclinedAndCommit(
        string? invalidReason,
        string? matrixTrackLabel)
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var targetRequestId = Guid.NewGuid();
        var professorId = Guid.NewGuid();

        var command = new ReviewSupervisionRequestCommand
        {
            ProjectInstanceId = targetInstanceId,
            SupervisionRequestId = targetRequestId,
            Accept = false,
            RejectionReason = invalidReason
        };

        var projectInstance = CreateMockProjectInstance(Guid.NewGuid(), ProjectInstanceStatus.AwaitingSupervision, professorId);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        var requestLog = projectInstance.SupervisionRequests.First();
        SetEntityPrivateProperty(requestLog, nameof(SupervisionRequest.Id), targetRequestId);
        SetEntityPrivateProperty(requestLog, nameof(SupervisionRequest.Status), SupervisionRequestStatus.Pending);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(professorId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        matrixTrackLabel.Should().NotBeNullOrWhiteSpace();
        result.Should().Be(Unit.Value);
        requestLog.Status.Should().Be(SupervisionRequestStatus.Rejected);
        requestLog.RejectionReason.Should().Be("Declined");

        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    #region Operational Aggregate Root Construction Factory Helpers

    /// <summary>
    /// Bypasses assembly linkage encapsulation blocks to construct a mock instance of the <see cref="ProjectInstance"/> aggregate root safely.
    /// </summary>
    private static ProjectInstance CreateMockProjectInstance(Guid studentId, ProjectInstanceStatus status, Guid? initialProfessorId)
    {
        var constructor = typeof(ProjectInstance).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            new[]
            {
                typeof(Guid), typeof(Guid), typeof(Guid), typeof(string), typeof(string),
                typeof(ProjectInstanceStatus), typeof(DateTime), typeof(Guid?), typeof(IEnumerable<Guid>)
            },
            null);

        if (constructor == null)
        {
            throw new InvalidOperationException("Critical Failure: Internal core parameter constructor layout signature matching failed.");
        }

        var instancesMock = (ProjectInstance)constructor.Invoke(new object?[]
        {
            studentId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Mock Snapshot Workspace Headline Title",
            "Mock Snapshot Workspace Scope Specification text content parameters description.",
            status,
            DateTime.UtcNow,
            initialProfessorId,
            Array.Empty<Guid>()
        });

        SetEntityPrivateProperty(instancesMock, nameof(ProjectInstance.Status), status);

        return instancesMock;
    }

    /// <summary>
    /// Sets private, back-end encapsulated aggregate root fields cleanly to support database entity key identity alignments.
    /// </summary>
    private static void SetEntityPrivateProperty<TEntity, TValue>(TEntity instance, string fieldLabel, TValue targetValue)
    {
        var targetPropertyInfo = typeof(TEntity).GetProperty(fieldLabel, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (targetPropertyInfo == null || !targetPropertyInfo.CanWrite)
        {
            var privateBackingFieldName = $"<{fieldLabel}>k__BackingField";
            var backingFieldInfo = typeof(TEntity).GetField(privateBackingFieldName, BindingFlags.Instance | BindingFlags.NonPublic);

            if (backingFieldInfo == null)
            {
                throw new ArgumentException($"Reflection Target Abort: Field string label reference matching '{fieldLabel}' failed extraction operations.");
            }

            backingFieldInfo.SetValue(instance, targetValue);
            return;
        }

        targetPropertyInfo.SetValue(instance, targetValue, null);
    }

    #endregion
}
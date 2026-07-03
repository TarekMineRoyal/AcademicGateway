using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProjectInstances.Commands.TransitionToSolo;
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

namespace AcademicGateway.Application.UnitTests.Features.ProjectInstances.Commands.TransitionToSolo;

/// <summary>
/// Contains completely isolated production-grade unit verification tests for the <see cref="TransitionToSoloCommandHandler"/>.
/// Validates data context retrieval filters, security credential tracking boundaries, student assignment guards,
/// state machine domain invariant checks, and atomic unit-of-work persistence loops.
/// </summary>
public class TransitionToSoloCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly TransitionToSoloCommandHandler _handler;

    /// <summary>
    /// Initializes a pristine instance of the <see cref="TransitionToSoloCommandHandlerTests"/> class with isolated tracking mocks.
    /// </summary>
    public TransitionToSoloCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _handler = new TransitionToSoloCommandHandler(_mockContext.Object, _mockCurrentUserService.Object);
    }

    /// <summary>
    /// Assures that when an authorized student transitions a workspace out of a paused matchmaking track, 
    /// outstanding pending requests are auto-rejected, status updates to Active, and transaction state commits cleanly.
    /// </summary>
    [Fact]
    public async Task Handle_GivenValidCommandAndAuthorizedStudentOwner_ShouldPivotToSoloTrackAndCommitRelationalChanges()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var authorizedStudentId = Guid.NewGuid();
        var pendingProfessorId = Guid.NewGuid();

        var command = new TransitionToSoloCommand
        {
            ProjectInstanceId = targetInstanceId
        };

        // Leverage the internal constructor to natively seed a pending SupervisionRequest record
        var projectInstance = CreateMockProjectInstance(
            authorizedStudentId,
            ProjectInstanceStatus.AwaitingSupervision,
            pendingProfessorId);

        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        // Verify the internal constructor properly initialized our target pending request state
        projectInstance.SupervisionRequests.Should().ContainSingle(r => r.Status == SupervisionRequestStatus.Pending);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(authorizedStudentId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(Unit.Value);
        projectInstance.Status.Should().Be(ProjectInstanceStatus.Active);
        projectInstance.SupervisorId.Should().BeNull();

        // Assert that outstanding matching records shifted cleanly to auto-rejection tracks
        var supervisionRecord = projectInstance.SupervisionRequests.First();
        supervisionRecord.Status.Should().Be(SupervisionRequestStatus.Rejected);
        supervisionRecord.RejectionReason.Should().Contain("Withdrawn by student shifting to a solo project");

        // Confirm the side-effect domain events got appended appropriately
        projectInstance.DomainEvents.Should().Contain(e => e.GetType().Name == "SupervisionRequestRejectedEvent");

        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that looking up a non-existent project workspace data key breaks execution flows 
    /// immediately and propagates a descriptive <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNonExistentProjectInstanceId_ShouldThrowKeyNotFoundExceptionAndAbortTransaction()
    {
        // Arrange
        var missingInstanceId = Guid.NewGuid();
        var command = new TransitionToSoloCommand { ProjectInstanceId = missingInstanceId };

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
    /// Assures that if an unauthenticated context attempts to execute a track transition flow command,
    /// an immediate security exception block is asserted by raising an <see cref="UnauthorizedAccessException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenUnauthenticatedUserContext_ShouldThrowUnauthorizedAccessExceptionAndBlockExecution()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var command = new TransitionToSoloCommand { ProjectInstanceId = targetInstanceId };

        var projectInstance = CreateMockProjectInstance(Guid.NewGuid(), ProjectInstanceStatus.AwaitingSupervision, null);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(false);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Access Denied: You are not authorized to alter the execution track of this project workspace.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if an authenticated student user maps against a project context owned by a separate student account,
    /// the security mapping checks halt mutation workflows with an <see cref="UnauthorizedAccessException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenAuthenticatedUserButNotTheStudentOwner_ShouldThrowUnauthorizedAccessExceptionAndProtectState()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var legitimateOwnerId = Guid.NewGuid();
        var unauthorizedUserContextId = Guid.NewGuid();

        var command = new TransitionToSoloCommand { ProjectInstanceId = targetInstanceId };

        var projectInstance = CreateMockProjectInstance(legitimateOwnerId, ProjectInstanceStatus.AwaitingSupervision, null);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(unauthorizedUserContextId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Access Denied: You are not authorized to alter the execution track of this project workspace.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if a workspace aggregate root has already left the matching limbo track layout configuration,
    /// core domain constraints trigger natively and bubble up an <see cref="InvalidProjectInstanceTransitionException"/>.
    /// </summary>
    /// <param name="ineligibleStatus">The non-applicable status code track parameters to inject.</param>
    /// <param name="matrixDiagnosisLabel">The nullable diagnosis string variable explicitly utilized to satisfy compiler warning rules.</param>
    [Theory]
    [InlineData(ProjectInstanceStatus.Active, "Live Running Active Track Layout Configuration")]
    [InlineData(ProjectInstanceStatus.Concluded, "Terminally Completed Frozen Track Layout Configuration")]
    [InlineData(ProjectInstanceStatus.Canceled, "Pre-emptively Abandoned and Aborted Track Layout Configuration")]
    public async Task Handle_GivenProjectInstanceInNonMatchingLimboStatus_ShouldPropagateInvalidProjectInstanceTransitionExceptionAndAbort(
        ProjectInstanceStatus ineligibleStatus,
        string? matrixDiagnosisLabel)
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var ownerStudentId = Guid.NewGuid();

        var command = new TransitionToSoloCommand { ProjectInstanceId = targetInstanceId };

        var projectInstance = CreateMockProjectInstance(ownerStudentId, ineligibleStatus, null);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(ownerStudentId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        matrixDiagnosisLabel.Should().NotBeNullOrWhiteSpace();
        await act.Should().ThrowAsync<InvalidProjectInstanceTransitionException>()
            .WithMessage($"*Cannot pivot to solo execution. Instance must be 'AwaitingSupervision', but is currently '{ineligibleStatus}'.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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

        // Ensure the internal backing status tracking replicates the exact test scenario state
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
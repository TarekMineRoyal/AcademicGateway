using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProjectInstances.Commands.CancelProject;
using AcademicGateway.Domain.ProjectInstances;
using AcademicGateway.Domain.ProjectInstances.Enums;
using AcademicGateway.Domain.ProjectInstances.Exceptions;
using FluentAssertions;
using MediatR;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.ProjectInstances.Commands.CancelProject;

/// <summary>
/// Contains completely isolated production-grade unit verification tests for the <see cref="CancelProjectCommandHandler"/>.
/// Validates infrastructure retrieval paths, security token state assertions, ownership verification boundaries, 
/// state machine domain invariant checks, and atomic relational storage completion loops.
/// </summary>
public class CancelProjectCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly CancelProjectCommandHandler _handler;

    /// <summary>
    /// Initializes a pristine instance of the <see cref="CancelProjectCommandHandlerTests"/> class with isolated tracking mocks.
    /// </summary>
    public CancelProjectCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _handler = new CancelProjectCommandHandler(_mockContext.Object, _mockCurrentUserService.Object);
    }

    /// <summary>
    /// Assures that when an authenticated student requests cancellation for a project workspace they own,
    /// the handler updates the aggregate status to Canceled and commits changes down to the database context layer.
    /// </summary>
    [Fact]
    public async Task Handle_GivenValidCommandAndAuthorizedStudentOwner_ShouldCancelWorkspaceAndCommitRelationalChanges()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var authorizedStudentId = Guid.NewGuid();

        var command = new CancelProjectCommand
        {
            ProjectInstanceId = targetInstanceId,
            Reason = "Decided to shift operational scope tracking priorities."
        };

        var projectInstance = CreateMockProjectInstance(authorizedStudentId, ProjectInstanceStatus.Active);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(authorizedStudentId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(Unit.Value);
        projectInstance.Status.Should().Be(ProjectInstanceStatus.Canceled);

        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that looking up an invalid or non-existent workspace instance database row identity record 
    /// cleanly interrupts execution flows and raises a descriptive <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNonExistentProjectInstanceId_ShouldThrowKeyNotFoundExceptionAndAbortTransaction()
    {
        // Arrange
        var missingInstanceId = Guid.NewGuid();
        var command = new CancelProjectCommand
        {
            ProjectInstanceId = missingInstanceId,
            Reason = null
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
    /// Assures that if an unauthenticated user context attempts to execute a workflow cancellation command, 
    /// an immediate security boundary violation is asserted via an <see cref="UnauthorizedAccessException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenUnauthenticatedUserContext_ShouldThrowUnauthorizedAccessExceptionAndBlockExecution()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var command = new CancelProjectCommand { ProjectInstanceId = targetInstanceId };

        var projectInstance = CreateMockProjectInstance(Guid.NewGuid(), ProjectInstanceStatus.Active);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(false);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Access Denied: Only the student owner can cancel this project workspace.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if an authenticated student attempts to cancel a workspace owned by a different student, 
    /// the security mapping checks block execution instantly with an <see cref="UnauthorizedAccessException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenAuthenticatedUserButNotTheStudentOwner_ShouldThrowUnauthorizedAccessExceptionAndProtectState()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var trackingOwnerId = Guid.NewGuid();
        var maliciousAttackerId = Guid.NewGuid();

        var command = new CancelProjectCommand { ProjectInstanceId = targetInstanceId };

        var projectInstance = CreateMockProjectInstance(trackingOwnerId, ProjectInstanceStatus.Active);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(maliciousAttackerId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Access Denied: Only the student owner can cancel this project workspace.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if a project workspace aggregate has already transitioned into an un-modifiable status track layout,
    /// domain constraint checking intercepts execution loops and bubbles up an <see cref="InvalidProjectInstanceTransitionException"/>.
    /// </summary>
    /// <param name="immutableStatus">The closed lifecycle state configuration parameters to inject.</param>
    /// <param name="matrixDiagnosisLabel">The nullable diagnosis string variable utilized explicitly to clear diagnostic analysis warnings.</param>
    [Theory]
    [InlineData(ProjectInstanceStatus.Canceled, "Workspace Pre-emptively Canceled Track Layout")]
    [InlineData(ProjectInstanceStatus.Concluded, "Workspace Terminally Concluded Track Layout")]
    public async Task Handle_GivenProjectWorkspaceInClosedStatus_ShouldPropagateInvalidProjectInstanceTransitionExceptionAndAbort(
        ProjectInstanceStatus immutableStatus,
        string? matrixDiagnosisLabel)
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var ownerStudentId = Guid.NewGuid();

        var command = new CancelProjectCommand { ProjectInstanceId = targetInstanceId };

        var projectInstance = CreateMockProjectInstance(ownerStudentId, immutableStatus);
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
            .WithMessage($"*Cannot cancel a workspace that is already closed out. Status: '{immutableStatus}'.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #region Operational Aggregate Root Construction Factory Helpers

    /// <summary>
    /// Bypasses assembly linkage encapsulation blocks to construct a mock instance of the <see cref="ProjectInstance"/> aggregate root safely.
    /// </summary>
    private static ProjectInstance CreateMockProjectInstance(Guid studentId, ProjectInstanceStatus status)
    {
        // Retrieve the non-public internal constructor to guarantee aggregate validation tracking rules remain clean
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
            null,
            Array.Empty<Guid>()
        });

        // Force set the initial state via reflection if the setup constructor maps complex defaults natively
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
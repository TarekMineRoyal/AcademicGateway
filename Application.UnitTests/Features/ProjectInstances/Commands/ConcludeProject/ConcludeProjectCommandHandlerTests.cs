using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProjectInstances.Commands.ConcludeProject;
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

namespace AcademicGateway.Application.UnitTests.Features.ProjectInstances.Commands.ConcludeProject;

/// <summary>
/// Contains completely isolated production-grade unit verification tests for the <see cref="ConcludeProjectCommandHandler"/>.
/// Validates storage retrieval tracks, identity context security mappings, student ownership alignment guards,
/// state machine domain constraint tracking, and atomic unit-of-work persistence boundaries.
/// </summary>
public class ConcludeProjectCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly ConcludeProjectCommandHandler _handler;

    /// <summary>
    /// Initializes a pristine instance of the <see cref="ConcludeProjectCommandHandlerTests"/> class with isolated tracking mocks.
    /// </summary>
    public ConcludeProjectCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _handler = new ConcludeProjectCommandHandler(_mockContext.Object, _mockCurrentUserService.Object);
    }

    /// <summary>
    /// Assures that when an authorized student owner calls conclude over a live workspace in an active or matching track state,
    /// the handler successfully transitions the aggregate status to Concluded and flushes operational rows down to storage.
    /// </summary>
    /// <param name="initialStatus">The eligible running status track code configuration to verify.</param>
    /// <param name="diagnosisTrackLabel">The nullable descriptor string variable passed explicitly to suppress code diagnostic analyzer warnings.</param>
    [Theory]
    [InlineData(ProjectInstanceStatus.Active, "Live Running Workspace Execution Track")]
    [InlineData(ProjectInstanceStatus.AwaitingSupervision, "Paused Matchmaking Supervisor Alignment Onboarding Track")]
    public async Task Handle_GivenValidCommandAndAuthorizedStudentOwner_ShouldConcludeWorkspaceAndCommitRelationalChanges(
        ProjectInstanceStatus initialStatus,
        string? diagnosisTrackLabel)
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var authorizedStudentId = Guid.NewGuid();

        var command = new ConcludeProjectCommand
        {
            ProjectInstanceId = targetInstanceId
        };

        var projectInstance = CreateMockProjectInstance(authorizedStudentId, initialStatus);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(authorizedStudentId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        diagnosisTrackLabel.Should().NotBeNullOrWhiteSpace();
        result.Should().Be(Unit.Value);
        projectInstance.Status.Should().Be(ProjectInstanceStatus.Concluded);

        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that looking up a non-existent workspace identity tracking key breaks handler routines 
    /// immediately and propagates a descriptive <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNonExistentProjectInstanceId_ShouldThrowKeyNotFoundExceptionAndAbortTransaction()
    {
        // Arrange
        var missingInstanceId = Guid.NewGuid();
        var command = new ConcludeProjectCommand { ProjectInstanceId = missingInstanceId };

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
    /// Assures that if an unauthenticated user context attempts to execute a conclusion workflow command, 
    /// a firm security boundary block is asserted by raising an <see cref="UnauthorizedAccessException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenUnauthenticatedUserContext_ShouldThrowUnauthorizedAccessExceptionAndBlockExecution()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var command = new ConcludeProjectCommand { ProjectInstanceId = targetInstanceId };

        var projectInstance = CreateMockProjectInstance(Guid.NewGuid(), ProjectInstanceStatus.Active);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(false);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Access Denied: Only the student owner can mark this project workspace as concluded.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if an authenticated student maps against a project workspace context owned by a separate student user account, 
    /// the security mapping checks halt execution with an <see cref="UnauthorizedAccessException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenAuthenticatedUserButNotTheStudentOwner_ShouldThrowUnauthorizedAccessExceptionAndProtectState()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var realOwnerStudentId = Guid.NewGuid();
        var maliciousUserContextId = Guid.NewGuid();

        var command = new ConcludeProjectCommand { ProjectInstanceId = targetInstanceId };

        var projectInstance = CreateMockProjectInstance(realOwnerStudentId, ProjectInstanceStatus.Active);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(maliciousUserContextId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Access Denied: Only the student owner can mark this project workspace as concluded.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if a workspace aggregate root has already transitioned into an un-modifiable closed state layout configuration,
    /// deep domain constraints catch the mutation attempt and bubble up an <see cref="InvalidProjectInstanceTransitionException"/>.
    /// </summary>
    /// <param name="immutableStatus">The terminal lifecycle status track parameters to test.</param>
    /// <param name="matrixDiagnosisLabel">The nullable diagnostic helper string used to satisfy compiler configuration warning parameters.</param>
    [Theory]
    [InlineData(ProjectInstanceStatus.Canceled, "Workspace Pre-emptively Aborted and Abandoned Track")]
    [InlineData(ProjectInstanceStatus.Concluded, "Workspace Terminally Frozen and Concluded Track")]
    public async Task Handle_GivenProjectWorkspaceInClosedImmutableStatus_ShouldPropagateInvalidProjectInstanceTransitionExceptionAndAbort(
        ProjectInstanceStatus immutableStatus,
        string? matrixDiagnosisLabel)
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var ownerStudentId = Guid.NewGuid();

        var command = new ConcludeProjectCommand { ProjectInstanceId = targetInstanceId };

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
        await act.Should().ThrowAsync<InvalidProjectInstanceTransitionException>();

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #region Operational Aggregate Root Construction Factory Helpers

    /// <summary>
    /// Bypasses assembly linkage encapsulation blocks to construct a mock instance of the <see cref="ProjectInstance"/> aggregate root safely.
    /// </summary>
    private static ProjectInstance CreateMockProjectInstance(Guid studentId, ProjectInstanceStatus status)
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
            null,
            Array.Empty<Guid>()
        });

        // Sync internal backing status trackers manually to replicate precise setup track layouts safely
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
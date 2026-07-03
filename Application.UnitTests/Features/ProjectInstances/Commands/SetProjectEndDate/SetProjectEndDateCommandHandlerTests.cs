using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProjectInstances.Commands.SetProjectEndDate;
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

namespace AcademicGateway.Application.UnitTests.Features.ProjectInstances.Commands.SetProjectEndDate;

/// <summary>
/// Contains completely isolated production-grade unit verification tests for the <see cref="SetProjectEndDateCommandHandler"/>.
/// Validates database entity retrieval paths, security token state mapping assertions, supervisor assignment verification boundaries,
/// state machine domain invariant checks, and atomic unit-of-work persistence loops.
/// </summary>
public class SetProjectEndDateCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly SetProjectEndDateCommandHandler _handler;

    /// <summary>
    /// Initializes a pristine instance of the <see cref="SetProjectEndDateCommandHandlerTests"/> class with isolated tracking mocks.
    /// </summary>
    public SetProjectEndDateCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _handler = new SetProjectEndDateCommandHandler(_mockContext.Object, _mockCurrentUserService.Object);
    }

    /// <summary>
    /// Assures that when an authenticated supervisor requests a deadline adjustment for an active project workspace they manage,
    /// the handler successfully updates the aggregate property and commits transaction updates down to data tracking layers.
    /// </summary>
    [Fact]
    public async Task Handle_GivenValidCommandAndAssignedSupervisor_ShouldSetProjectEndDateAndCommitRelationalChanges()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var authorizedSupervisorId = Guid.NewGuid();
        var validFutureDate = DateTime.UtcNow.AddDays(14);

        var command = new SetProjectEndDateCommand
        {
            ProjectInstanceId = targetInstanceId,
            NewEndDate = validFutureDate
        };

        var projectInstance = CreateMockProjectInstance(Guid.NewGuid(), ProjectInstanceStatus.Active);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.SupervisorId), authorizedSupervisorId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(authorizedSupervisorId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(Unit.Value);
        projectInstance.EndDate.Should().Be(validFutureDate);

        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that looking up a non-existent project workspace aggregate tracking row key isolates execution 
    /// routines cleanly and propagates a descriptive <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNonExistentProjectInstanceId_ShouldThrowKeyNotFoundExceptionAndAbortTransaction()
    {
        // Arrange
        var missingInstanceId = Guid.NewGuid();
        var command = new SetProjectEndDateCommand
        {
            ProjectInstanceId = missingInstanceId,
            NewEndDate = DateTime.UtcNow.AddDays(5)
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
    /// Assures that if an unauthenticated user context profile attempts to execute a workflow deadline mutation command,
    /// a perimeter security boundary block is asserted by throwing an <see cref="UnauthorizedAccessException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenUnauthenticatedUserContext_ShouldThrowUnauthorizedAccessExceptionAndBlockExecution()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var command = new SetProjectEndDateCommand
        {
            ProjectInstanceId = targetInstanceId,
            NewEndDate = DateTime.UtcNow.AddDays(5)
        };

        var projectInstance = CreateMockProjectInstance(Guid.NewGuid(), ProjectInstanceStatus.Active);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(false);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Access Denied: You must be authenticated to alter workspace deadlines.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if a project workspace aggregate root is in a non-active lifecycle tracking track,
    /// core domain assertions trigger immediately, bubbling up an <see cref="InvalidProjectInstanceTransitionException"/>.
    /// </summary>
    /// <param name="nonActiveStatus">The ineligible lifecycle status track parameters to test.</param>
    /// <param name="matrixTrackLabel">The nullable diagnosis helper string utilized explicitly to clear diagnostic analysis warnings.</param>
    [Theory]
    [InlineData(ProjectInstanceStatus.AwaitingSupervision, "Paused Matchmaking Alignment Track")]
    [InlineData(ProjectInstanceStatus.Concluded, "Terminally Completed Track")]
    [InlineData(ProjectInstanceStatus.Canceled, "Pre-emptively Aborted Track")]
    public async Task Handle_GivenProjectInstanceInNonActiveStatus_ShouldPropagateInvalidProjectInstanceTransitionExceptionAndAbort(
        ProjectInstanceStatus nonActiveStatus,
        string? matrixTrackLabel)
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var supervisorId = Guid.NewGuid();
        var command = new SetProjectEndDateCommand
        {
            ProjectInstanceId = targetInstanceId,
            NewEndDate = DateTime.UtcNow.AddDays(5)
        };

        var projectInstance = CreateMockProjectInstance(Guid.NewGuid(), nonActiveStatus);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.SupervisorId), supervisorId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(supervisorId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        matrixTrackLabel.Should().NotBeNullOrWhiteSpace();
        await act.Should().ThrowAsync<InvalidProjectInstanceTransitionException>()
            .WithMessage("*Deadlines can only be assigned to live, active workspace channels.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if an authenticated user attempts to change deadlines on an active project but is not the officially 
    /// assigned academic mentor for that instance, access controls reject the operation via an <see cref="InvalidProjectInstanceTransitionException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenAuthenticatedUserButNotTheAssignedSupervisor_ShouldThrowInvalidProjectInstanceTransitionExceptionAndProtectTimeline()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var legitimateSupervisorId = Guid.NewGuid();
        var unauthorizedProfessorId = Guid.NewGuid();

        var command = new SetProjectEndDateCommand
        {
            ProjectInstanceId = targetInstanceId,
            NewEndDate = DateTime.UtcNow.AddDays(5)
        };

        var projectInstance = CreateMockProjectInstance(Guid.NewGuid(), ProjectInstanceStatus.Active);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.SupervisorId), legitimateSupervisorId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(unauthorizedProfessorId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidProjectInstanceTransitionException>()
            .WithMessage("*Access Denied: Only the assigned academic supervisor can manage timeline limitations on this instance.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that when the current user context is authenticated but passes a null tracking layout key,
    /// the safe null-coalescing operation maps a default value, causing standard supervisor lookup guards to throw an exception.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNullUserIdContext_ShouldCoalesceToEmptyGuidAndThrowInvalidProjectInstanceTransitionException()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var command = new SetProjectEndDateCommand
        {
            ProjectInstanceId = targetInstanceId,
            NewEndDate = DateTime.UtcNow.AddDays(5)
        };

        var projectInstance = CreateMockProjectInstance(Guid.NewGuid(), ProjectInstanceStatus.Active);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.SupervisorId), Guid.NewGuid()); // Distinct from empty

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns((Guid?)null); // Forces ?? Guid.Empty execution pathway branch

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidProjectInstanceTransitionException>()
            .WithMessage("*Access Denied: Only the assigned academic supervisor can manage timeline limitations on this instance.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that passing an obsolete calendar deadline date falling in the past or present relative to the system execution tracker 
    /// natively triggers deep domain aggregate guards and raises an <see cref="InvalidProjectInstanceTransitionException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenProposedEndDateInThePast_ShouldPropagateInvalidProjectInstanceTransitionExceptionAndAbort()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var supervisorId = Guid.NewGuid();
        var obsoletePastDate = DateTime.UtcNow.AddDays(-1); // Invalid parameter value checks boundary

        var command = new SetProjectEndDateCommand
        {
            ProjectInstanceId = targetInstanceId,
            NewEndDate = obsoletePastDate
        };

        var projectInstance = CreateMockProjectInstance(Guid.NewGuid(), ProjectInstanceStatus.Active);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.SupervisorId), supervisorId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(supervisorId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidProjectInstanceTransitionException>()
            .WithMessage("*The operational end boundary date must exist in the future.*");

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
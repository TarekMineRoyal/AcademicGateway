using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.SupervisionRequests.Commands.SubmitSupervisionRequest;
using AcademicGateway.Domain.Professors;
using AcademicGateway.Domain.ProjectInstances;
using AcademicGateway.Domain.ProjectInstances.Enums;
using AcademicGateway.Domain.ProjectInstances.Exceptions;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.SupervisionRequests.Commands.SubmitSupervisionRequest;

/// <summary>
/// Contains completely isolated production-grade unit verification tests for the <see cref="SubmitSupervisionRequestCommandHandler"/>.
/// Validates storage retrieval filters, security credential boundaries, relational dependency check layers, 
/// state machine domain invariant guards, and atomic transactional persistence executions.
/// </summary>
public class SubmitSupervisionRequestCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IDateTimeProvider> _mockDateTimeProvider;
    private readonly SubmitSupervisionRequestCommandHandler _handler;
    private readonly DateTime _deterministicUtcNow;

    /// <summary>
    /// Initializes a pristine instance of the <see cref="SubmitSupervisionRequestCommandHandlerTests"/> class with isolated tracking mocks.
    /// </summary>
    public SubmitSupervisionRequestCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockDateTimeProvider = new Mock<IDateTimeProvider>();

        _handler = new SubmitSupervisionRequestCommandHandler(
            _mockContext.Object,
            _mockCurrentUserService.Object,
            _mockDateTimeProvider.Object);

        _deterministicUtcNow = new DateTime(2026, 7, 3, 10, 30, 0, DateTimeKind.Utc);
        _mockDateTimeProvider.Setup(d => d.UtcNow).Returns(_deterministicUtcNow);
    }

    /// <summary>
    /// Assures that when an authorized student student owner submits a valid supervision request to a verified professor, 
    /// the handler successfully updates the aggregate child rows, appends events, and saves atomically.
    /// </summary>
    [Fact]
    public async Task Handle_GivenValidCommandAndAuthorizedOwner_ShouldCreateSupervisionRequestAndCommitToDatabase()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var authorizedStudentId = Guid.NewGuid();
        var targetProfessorId = Guid.NewGuid();

        var command = new SubmitSupervisionRequestCommand
        {
            ProjectInstanceId = targetInstanceId,
            ProfessorId = targetProfessorId,
            PitchText = "Seeking mentorship for my high-throughput stream-processing telemetry engine workspace project loop."
        };

        var projectInstance = CreateMockProjectInstance(authorizedStudentId, ProjectInstanceStatus.Active);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        // Enforce valid professor profile mapping constraint presence
        var professor = CreateMockProfessor(targetProfessorId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(authorizedStudentId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.Professors)
            .Returns(new List<Professor> { professor }.BuildMockDbSet().Object);

        // Act
        var resultRequestGuid = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        resultRequestGuid.Should().NotBeEmpty();
        projectInstance.SupervisionRequests.Should().ContainSingle();

        var spawnedRequest = projectInstance.SupervisionRequests.First();
        spawnedRequest.Id.Should().Be(resultRequestGuid);
        spawnedRequest.ProfessorId.Should().Be(targetProfessorId);
        spawnedRequest.PitchText.Should().Be(command.PitchText);
        spawnedRequest.Status.Should().Be(SupervisionRequestStatus.Pending);
        spawnedRequest.CreatedAt.Should().Be(_deterministicUtcNow);

        // Assert that the proper domain event got appended safely onto our tracking matrix
        projectInstance.DomainEvents.Should().Contain(e => e.GetType().Name == "SupervisionRequestCreatedEvent");

        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that looking up a non-existent project workspace data tracking key interrupts handler routines 
    /// immediately and propagates a descriptive <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNonExistentProjectInstanceId_ShouldThrowKeyNotFoundExceptionAndAbortTransaction()
    {
        // Arrange
        var missingInstanceId = Guid.NewGuid();
        var command = new SubmitSupervisionRequestCommand
        {
            ProjectInstanceId = missingInstanceId,
            ProfessorId = Guid.NewGuid(),
            PitchText = "Valid Text content parameters specifications."
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
    /// Assures that if an unauthenticated user session context attempts to submit a supervision invitation request, 
    /// a perimeter security boundary block is asserted by throwing an <see cref="UnauthorizedAccessException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenUnauthenticatedUserContext_ShouldThrowUnauthorizedAccessExceptionAndBlockExecution()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var command = new SubmitSupervisionRequestCommand { ProjectInstanceId = targetInstanceId, ProfessorId = Guid.NewGuid() };

        var projectInstance = CreateMockProjectInstance(Guid.NewGuid(), ProjectInstanceStatus.Active);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(false);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Access Denied: You can only submit academic supervision requests for project instances that you own.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if an authenticated student maps against a project workspace owned by a different student user profile, 
    /// the security mapping checks halt execution with an <see cref="UnauthorizedAccessException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenAuthenticatedUserButNotTheStudentOwner_ShouldThrowUnauthorizedAccessExceptionAndProtectState()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var realOwnerId = Guid.NewGuid();
        var alienUserId = Guid.NewGuid();

        var command = new SubmitSupervisionRequestCommand { ProjectInstanceId = targetInstanceId, ProfessorId = Guid.NewGuid() };

        var projectInstance = CreateMockProjectInstance(realOwnerId, ProjectInstanceStatus.Active);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(alienUserId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Access Denied: You can only submit academic supervision requests for project instances that you own.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that attempting to route a supervision invitation request to a non-existent professor 
    /// breaks the relational constraint validation and propagates a descriptive <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNonExistentProfessorId_ShouldThrowKeyNotFoundExceptionAndAbort()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var authorizedStudentId = Guid.NewGuid();
        var wrongProfessorId = Guid.NewGuid();

        var command = new SubmitSupervisionRequestCommand
        {
            ProjectInstanceId = targetInstanceId,
            ProfessorId = wrongProfessorId,
            PitchText = "Valid motivation criteria summary description text parameters."
        };

        var projectInstance = CreateMockProjectInstance(authorizedStudentId, ProjectInstanceStatus.Active);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(authorizedStudentId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.Professors)
            .Returns(new List<Professor>().BuildMockDbSet().Object); // Empty database context set branch track

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*The targeted academic professor profile with ID '{wrongProfessorId}' does not exist.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if a project workspace aggregate has already transitioned into an un-modifiable closed state configuration status track,
    /// core domain assertions intercept execution loops and bubble up an <see cref="InvalidProjectInstanceTransitionException"/>.
    /// </summary>
    /// <param name="closedStatus">The immutable status code tracking parameters to test.</param>
    /// <param name="matrixDiagnosisLabel">The nullable diagnosis helper string utilized explicitly to clear diagnostic analysis warnings.</param>
    [Theory]
    [InlineData(ProjectInstanceStatus.Concluded, "Frozen Concluded Workspace Layout Track")]
    [InlineData(ProjectInstanceStatus.Canceled, "Aborted Canceled Workspace Layout Track")]
    public async Task Handle_GivenProjectInstanceInClosedStatus_ShouldPropagateInvalidProjectInstanceTransitionExceptionAndAbort(
        ProjectInstanceStatus closedStatus,
        string? matrixDiagnosisLabel)
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var authorizedStudentId = Guid.NewGuid();
        var targetProfessorId = Guid.NewGuid();

        var command = new SubmitSupervisionRequestCommand { ProjectInstanceId = targetInstanceId, ProfessorId = targetProfessorId };

        var projectInstance = CreateMockProjectInstance(authorizedStudentId, closedStatus);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        var professor = CreateMockProfessor(targetProfessorId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(authorizedStudentId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.Professors)
            .Returns(new List<Professor> { professor }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        matrixDiagnosisLabel.Should().NotBeNullOrWhiteSpace();
        await act.Should().ThrowAsync<InvalidProjectInstanceTransitionException>()
            .WithMessage($"*Cannot request supervision. The project workspace is currently '{closedStatus}'.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if a project workspace aggregate root is already bound to an active supervisor,
    /// deep domain guards intercept processing lines natively and bubble up an <see cref="InvalidProjectInstanceTransitionException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenProjectInstanceWithExistingSupervisor_ShouldThrowInvalidProjectInstanceTransitionExceptionAndAbort()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var authorizedStudentId = Guid.NewGuid();
        var targetProfessorId = Guid.NewGuid();

        var command = new SubmitSupervisionRequestCommand { ProjectInstanceId = targetInstanceId, ProfessorId = targetProfessorId };

        var projectInstance = CreateMockProjectInstance(authorizedStudentId, ProjectInstanceStatus.Active);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.SupervisorId), Guid.NewGuid()); // Already bound tracking key

        var professor = CreateMockProfessor(targetProfessorId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(authorizedStudentId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.Professors)
            .Returns(new List<Professor> { professor }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidProjectInstanceTransitionException>()
            .WithMessage("*This project instance is already bound to an active academic supervisor.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if a project workspace aggregate root already contains a pending supervision request record,
    /// deep domain invariants reject the duplicate submission to protect faculty backlog review volumes.
    /// </summary>
    [Fact]
    public async Task Handle_GivenProjectInstanceWithPreexistingPendingSupervisionRequest_ShouldThrowInvalidProjectInstanceTransitionExceptionAndAbort()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var authorizedStudentId = Guid.NewGuid();
        var targetProfessorId = Guid.NewGuid();
        var preExistingProfessorId = Guid.NewGuid();

        var command = new SubmitSupervisionRequestCommand { ProjectInstanceId = targetInstanceId, ProfessorId = targetProfessorId };

        // Seed an active pending item natively through constructor flows
        var projectInstance = CreateMockProjectInstance(authorizedStudentId, ProjectInstanceStatus.AwaitingSupervision, preExistingProfessorId);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Status), ProjectInstanceStatus.Active); // Override status back to active to isolate request status guard path

        var professor = CreateMockProfessor(targetProfessorId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(authorizedStudentId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.Professors)
            .Returns(new List<Professor> { professor }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidProjectInstanceTransitionException>()
            .WithMessage("*An active supervision request is already pending review for this project workspace.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #region Operational Aggregate Root Construction Factory Helpers

    /// <summary>
    /// Bypasses assembly linkage encapsulation blocks to construct a mock instance of the <see cref="ProjectInstance"/> aggregate root safely.
    /// </summary>
    private static ProjectInstance CreateMockProjectInstance(Guid studentId, ProjectInstanceStatus status, Guid? initialProfessorId = null)
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
    /// Bypasses assembly encapsulation constraints to generate a mock instance of the <see cref="Professor"/> profile root cleanly.
    /// </summary>
    private static Professor CreateMockProfessor(Guid professorId)
    {
        var constructor = typeof(Professor).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            null,
            Type.EmptyTypes,
            null);

        var instance = constructor != null
            ? (Professor)constructor.Invoke(null)
            : (Professor)RuntimeHelpers.GetUninitializedObject(typeof(Professor));

        SetEntityPrivateProperty(instance, nameof(Professor.Id), professorId);
        return instance;
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
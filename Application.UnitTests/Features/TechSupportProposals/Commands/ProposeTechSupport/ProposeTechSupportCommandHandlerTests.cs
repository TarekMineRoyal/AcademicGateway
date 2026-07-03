using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.TechSupportProposals.Commands.ProposeTechSupport;
using AcademicGateway.Domain.ProjectInstances;
using AcademicGateway.Domain.ProjectInstances.Enums;
using AcademicGateway.Domain.ProjectInstances.Exceptions;
using AcademicGateway.Domain.Providers;
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

namespace AcademicGateway.Application.UnitTests.Features.TechSupportProposals.Commands.ProposeTechSupport;

/// <summary>
/// Contains completely isolated production-grade unit verification tests for the <see cref="ProposeTechSupportCommandHandler"/>.
/// Validates data context lookup streams, multi-tenant corporate tenancy boundaries, identity role permission mapping checks,
/// aggregate root state machine constraint checking, and transactional persistence flows.
/// </summary>
public class ProposeTechSupportCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IDateTimeProvider> _mockDateTimeProvider;
    private readonly ProposeTechSupportCommandHandler _handler;
    private readonly DateTime _deterministicUtcNow;

    /// <summary>
    /// Initializes a pristine instance of the <see cref="ProposeTechSupportCommandHandlerTests"/> class with isolated tracking mocks.
    /// </summary>
    public ProposeTechSupportCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockDateTimeProvider = new Mock<IDateTimeProvider>();

        _handler = new ProposeTechSupportCommandHandler(
            _mockContext.Object,
            _mockCurrentUserService.Object,
            _mockDateTimeProvider.Object);

        _deterministicUtcNow = new DateTime(2026, 7, 3, 15, 45, 0, DateTimeKind.Utc);
        _mockDateTimeProvider.Setup(d => d.UtcNow).Returns(_deterministicUtcNow);
    }

    /// <summary>
    /// Assures that when an authorized template provider corporate account hooks a technical mentor support account 
    /// into a live active student workspace, a pending assignment log is added and saved atomically.
    /// </summary>
    [Fact]
    public async Task Handle_GivenValidCommandAndAuthorizedProviderOwner_ShouldCreateTechSupportProposalAndCommitToDatabase()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var authorizedProviderId = Guid.NewGuid();
        var targetTechSupportAccountId = Guid.NewGuid();

        var command = new ProposeTechSupportCommand
        {
            ProjectInstanceId = targetInstanceId,
            TechSupportAccountId = targetTechSupportAccountId,
            Message = "Introducing a Senior Systems Architect to mentor the workspace stream topology."
        };

        var projectInstance = CreateMockProjectInstance(Guid.NewGuid(), authorizedProviderId, ProjectInstanceStatus.Active);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        var supportAccount = CreateMockTechSupportAccount(targetTechSupportAccountId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(authorizedProviderId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.TechSupportAccounts)
            .Returns(new List<TechSupportAccount> { supportAccount }.BuildMockDbSet().Object);

        // Act
        var resultProposalId = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        resultProposalId.Should().NotBeEmpty();
        projectInstance.TechSupportProposals.Should().ContainSingle();

        var spawnedProposal = projectInstance.TechSupportProposals.First();
        spawnedProposal.TechSupportAccountId.Should().Be(targetTechSupportAccountId);
        spawnedProposal.Status.Should().Be(TechSupportProposalStatus.Pending);

        // Verify domain event registration mechanics within the parent tracking queue
        projectInstance.DomainEvents.Should().Contain(e => e.GetType().Name == "TechSupportProposalCreatedEvent");

        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that looking up an invalid or missing project workspace row key isolates handler routines 
    /// cleanly and propagates a descriptive <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNonExistentProjectInstanceId_ShouldThrowKeyNotFoundExceptionAndAbortTransaction()
    {
        // Arrange
        var missingInstanceId = Guid.NewGuid();
        var command = new ProposeTechSupportCommand
        {
            ProjectInstanceId = missingInstanceId,
            TechSupportAccountId = Guid.NewGuid(),
            Message = "Valid introductory notes."
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
    /// Assures that if an unauthenticated user token context profile triggers a proposal command, 
    /// an immediate perimeter security exception is thrown via an <see cref="UnauthorizedAccessException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenUnauthenticatedUserContext_ShouldThrowUnauthorizedAccessExceptionAndBlockExecution()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var command = new ProposeTechSupportCommand { ProjectInstanceId = targetInstanceId, TechSupportAccountId = Guid.NewGuid() };

        var projectInstance = CreateMockProjectInstance(Guid.NewGuid(), Guid.NewGuid(), ProjectInstanceStatus.Active);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(false);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Access Denied: You are not authorized to assign technical support personnel to this project workspace.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if an authenticated corporate account attempts to pitch mentorship to a workspace owned by 
    /// a separate company user profile, corporate multi-tenant boundary checks block execution with an <see cref="UnauthorizedAccessException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenAuthenticatedProviderButNotTheTemplateOwner_ShouldThrowUnauthorizedAccessExceptionAndProtectState()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var legitimateProviderOwnerId = Guid.NewGuid();
        var alienIntruderProviderId = Guid.NewGuid();

        var command = new ProposeTechSupportCommand { ProjectInstanceId = targetInstanceId, TechSupportAccountId = Guid.NewGuid() };

        var projectInstance = CreateMockProjectInstance(Guid.NewGuid(), legitimateProviderOwnerId, ProjectInstanceStatus.Active);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(alienIntruderProviderId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Access Denied: You are not authorized to assign technical support personnel to this project workspace.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that referencing an invalid or non-existent technical support profile breaks relational pre-checks
    /// and throws a descriptive <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNonExistentTechSupportAccountId_ShouldThrowKeyNotFoundExceptionAndAbort()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var authorizedProviderId = Guid.NewGuid();
        var wrongSupportId = Guid.NewGuid();

        var command = new ProposeTechSupportCommand
        {
            ProjectInstanceId = targetInstanceId,
            TechSupportAccountId = wrongSupportId,
            Message = "Introductory context lines."
        };

        var projectInstance = CreateMockProjectInstance(Guid.NewGuid(), authorizedProviderId, ProjectInstanceStatus.Active);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(authorizedProviderId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.TechSupportAccounts)
            .Returns(new List<TechSupportAccount>().BuildMockDbSet().Object); // Empty table mapping path

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*The corporate tech support profile with ID '{wrongSupportId}' does not exist.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if a project workspace aggregate has transitioned out of an Active state timeline track,
    /// domain invariant rules block the proposal and throw an <see cref="InvalidProjectInstanceTransitionException"/>.
    /// </summary>
    /// <param name="ineligibleStatus">The ineligible lifecycle status track parameters to verify.</param>
    /// <param name="matrixTrackLabel">The nullable diagnosis helper string utilized explicitly to clear diagnostic analysis warnings.</param>
    [Theory]
    [InlineData(ProjectInstanceStatus.AwaitingSupervision, "Paused Matchmaking Alignment Track Configuration Layout")]
    [InlineData(ProjectInstanceStatus.Concluded, "Terminally Completed Frozen Track Configuration Layout")]
    [InlineData(ProjectInstanceStatus.Canceled, "Pre-emptively Abandoned Aborted Track Configuration Layout")]
    public async Task Handle_GivenProjectInstanceInNonActiveStatus_ShouldPropagateInvalidProjectInstanceTransitionExceptionAndAbort(
        ProjectInstanceStatus ineligibleStatus,
        string? matrixTrackLabel)
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var authorizedProviderId = Guid.NewGuid();
        var targetTechSupportAccountId = Guid.NewGuid();

        var command = new ProposeTechSupportCommand { ProjectInstanceId = targetInstanceId, TechSupportAccountId = targetTechSupportAccountId };

        var projectInstance = CreateMockProjectInstance(Guid.NewGuid(), authorizedProviderId, ineligibleStatus);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        var supportAccount = CreateMockTechSupportAccount(targetTechSupportAccountId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(authorizedProviderId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.TechSupportAccounts)
            .Returns(new List<TechSupportAccount> { supportAccount }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        matrixTrackLabel.Should().NotBeNullOrWhiteSpace();
        await act.Should().ThrowAsync<InvalidProjectInstanceTransitionException>()
            .WithMessage($"*Corporate mentors can only be attached to active running instances. Current status: '{ineligibleStatus}'.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if a project workspace already houses a concurrent pending assistance offer directed from the exact same tech support account,
    /// aggregate validation rules reject the duplicate submission to protect student feedback workflows.
    /// </summary>
    [Fact]
    public async Task Handle_GivenProjectInstanceWithPreexistingPendingProposalForSameAccount_ShouldThrowInvalidProjectInstanceTransitionExceptionAndAbort()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var authorizedProviderId = Guid.NewGuid();
        var duplicateTechSupportAccountId = Guid.NewGuid();

        var command = new ProposeTechSupportCommand { ProjectInstanceId = targetInstanceId, TechSupportAccountId = duplicateTechSupportAccountId };

        var projectInstance = CreateMockProjectInstance(Guid.NewGuid(), authorizedProviderId, ProjectInstanceStatus.Active);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        // Core aggregate execution setup track path mapping to seed the preexisting item natively
        projectInstance.ProposeTechSupport(duplicateTechSupportAccountId, _deterministicUtcNow);

        // Reassert our workspace status context layer falls back to active track cleanly after internal modifications
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Status), ProjectInstanceStatus.Active);

        var supportAccount = CreateMockTechSupportAccount(duplicateTechSupportAccountId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(authorizedProviderId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.TechSupportAccounts)
            .Returns(new List<TechSupportAccount> { supportAccount }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidProjectInstanceTransitionException>()
            .WithMessage("*A pending corporate assistance offer for this specific tech support account is already awaiting student feedback.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #region Operational Aggregate Root Construction Factory Helpers

    /// <summary>
    /// Bypasses assembly linkage encapsulation blocks to construct a mock instance of the <see cref="ProjectInstance"/> aggregate root safely.
    /// </summary>
    private static ProjectInstance CreateMockProjectInstance(Guid studentId, Guid providerId, ProjectInstanceStatus status)
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
            providerId,
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
    /// Bypasses assembly encapsulation constraints to generate a mock instance of the <see cref="TechSupportAccount"/> root cleanly.
    /// </summary>
    private static TechSupportAccount CreateMockTechSupportAccount(Guid supportId)
    {
        var constructor = typeof(TechSupportAccount).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            null,
            Type.EmptyTypes,
            null);

        var instance = constructor != null
            ? (TechSupportAccount)constructor.Invoke(null)
            : (TechSupportAccount)RuntimeHelpers.GetUninitializedObject(typeof(TechSupportAccount));

        SetEntityPrivateProperty(instance, nameof(TechSupportAccount.Id), supportId);
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
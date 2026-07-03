using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.TechSupportProposals.Commands.ReviewTechSupportProposal;
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

namespace AcademicGateway.Application.UnitTests.Features.TechSupportProposals.Commands.ReviewTechSupportProposal;

/// <summary>
/// Contains completely isolated production-grade unit verification tests for the <see cref="ReviewTechSupportProposalCommandHandler"/>.
/// Validates storage entity graph parsing filters, security context identity checks, tenancy student ownership boundaries, 
/// aggregate root state machine tracking invariants, and transaction persistence flow completions.
/// </summary>
public class ReviewTechSupportProposalCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly ReviewTechSupportProposalCommandHandler _handler;

    /// <summary>
    /// Initializes a pristine instance of the <see cref="ReviewTechSupportProposalCommandHandlerTests"/> class with isolated tracking mocks.
    /// </summary>
    public ReviewTechSupportProposalCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _handler = new ReviewTechSupportProposalCommandHandler(_mockContext.Object, _mockCurrentUserService.Object);
    }

    /// <summary>
    /// Assures that when a verified student workspace owner accepts a corporate mentorship assistance offer, 
    /// the child proposal status transitions to Accepted, corresponding events append, and records flush down atomically.
    /// </summary>
    [Fact]
    public async Task Handle_GivenValidCommandWithAcceptTrue_ShouldAcceptTechSupportProposalAndCommitRelationalChanges()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var targetProposalId = Guid.NewGuid();
        var authorizedStudentOwnerId = Guid.NewGuid();
        var corporateTechSupportAccountId = Guid.NewGuid();

        var command = new ReviewTechSupportProposalCommand
        {
            ProjectInstanceId = targetInstanceId,
            TechSupportProposalId = targetProposalId,
            Accept = true,
            RejectionReason = null
        };

        // Construct parent aggregate in Active status so it can naturally receive a corporate proposal
        var projectInstance = CreateMockProjectInstance(authorizedStudentOwnerId, ProjectInstanceStatus.Active);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        // Seed the proposal child entity using authentic domain workflow mechanics to ensure encapsulation
        projectInstance.ProposeTechSupport(corporateTechSupportAccountId, DateTime.UtcNow);

        var spawnedProposal = projectInstance.TechSupportProposals.First();
        SetEntityPrivateProperty(spawnedProposal, nameof(TechSupportProposal.Id), targetProposalId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(authorizedStudentOwnerId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(Unit.Value);
        spawnedProposal.Status.Should().Be(TechSupportProposalStatus.Accepted);

        // Assert the side-effect notification event was registered into our parent aggregate root tracking queue
        projectInstance.DomainEvents.Should().Contain(e => e.GetType().Name == "TechSupportProposalAcceptedEvent");

        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that when a verified student workspace owner declines a corporate mentorship offer while supplying text commentary, 
    /// the child proposal status drops to Rejected, notes are captured, and changes save down atomically.
    /// </summary>
    [Fact]
    public async Task Handle_GivenValidCommandWithAcceptFalse_ShouldRejectTechSupportProposalAndCommitRelationalChanges()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var targetProposalId = Guid.NewGuid();
        var authorizedStudentOwnerId = Guid.NewGuid();
        var corporateTechSupportAccountId = Guid.NewGuid();

        var command = new ReviewTechSupportProposalCommand
        {
            ProjectInstanceId = targetInstanceId,
            TechSupportProposalId = targetProposalId,
            Accept = false,
            RejectionReason = "Our structural timeline tracking architecture is already fully covered by our academic mentor."
        };

        var projectInstance = CreateMockProjectInstance(authorizedStudentOwnerId, ProjectInstanceStatus.Active);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        projectInstance.ProposeTechSupport(corporateTechSupportAccountId, DateTime.UtcNow);
        var spawnedProposal = projectInstance.TechSupportProposals.First();
        SetEntityPrivateProperty(spawnedProposal, nameof(TechSupportProposal.Id), targetProposalId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(authorizedStudentOwnerId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(Unit.Value);
        spawnedProposal.Status.Should().Be(TechSupportProposalStatus.Rejected);
        spawnedProposal.RejectionReason.Should().Be(command.RejectionReason);

        // Assert the side-effect notification event was registered into our parent aggregate root tracking queue
        projectInstance.DomainEvents.Should().Contain(e => e.GetType().Name == "TechSupportProposalRejectedEvent");

        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that looking up a non-existent project workspace tracking key isolates handler execution 
    /// routines cleanly and propagates a descriptive <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNonExistentProjectInstanceId_ShouldThrowKeyNotFoundExceptionAndAbortTransaction()
    {
        // Arrange
        var missingInstanceId = Guid.NewGuid();
        var command = new ReviewTechSupportProposalCommand
        {
            ProjectInstanceId = missingInstanceId,
            TechSupportProposalId = Guid.NewGuid(),
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
    /// Assures that looking up a corporate assistance proposal record key that does not reside inside the targeted project's 
    /// child array collection boundary throws a descriptive <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNonExistentTechSupportProposalId_ShouldThrowKeyNotFoundExceptionAndProtectState()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var missingProposalId = Guid.NewGuid();
        var studentOwnerId = Guid.NewGuid();

        var command = new ReviewTechSupportProposalCommand
        {
            ProjectInstanceId = targetInstanceId,
            TechSupportProposalId = missingProposalId,
            Accept = true
        };

        var projectInstance = CreateMockProjectInstance(studentOwnerId, ProjectInstanceStatus.Active);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(studentOwnerId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*Corporate mentor proposal with tracking key '{missingProposalId}' was not found in this workspace context.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if an unauthenticated user context profile tries to trigger a evaluation selection command, 
    /// perimeter access guards block the pipeline immediately with an <see cref="UnauthorizedAccessException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenUnauthenticatedUserContext_ShouldThrowUnauthorizedAccessExceptionAndBlockExecution()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var targetProposalId = Guid.NewGuid();

        var command = new ReviewTechSupportProposalCommand
        {
            ProjectInstanceId = targetInstanceId,
            TechSupportProposalId = targetProposalId,
            Accept = true
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
            .WithMessage("*Access Denied: You are not authorized to evaluate corporate assistance offers for this workspace.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if an authenticated student account attempts to evaluate an assistance proposal bound to a separate student's workspace, 
    /// student multi-tenancy access boundary protections raise an <see cref="UnauthorizedAccessException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenAuthenticatedUserButNotTheStudentOwner_ShouldThrowUnauthorizedAccessExceptionAndProtectState()
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var targetProposalId = Guid.NewGuid();
        var realOwnerStudentId = Guid.NewGuid();
        var maliciousIntruderStudentId = Guid.NewGuid();

        var command = new ReviewTechSupportProposalCommand
        {
            ProjectInstanceId = targetInstanceId,
            TechSupportProposalId = targetProposalId,
            Accept = true
        };

        var projectInstance = CreateMockProjectInstance(realOwnerStudentId, ProjectInstanceStatus.Active);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(maliciousIntruderStudentId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Access Denied: You are not authorized to evaluate corporate assistance offers for this workspace.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if a corporate assistance proposal tracking record has left a pending state configuration track, 
    /// core state transitions natively intercept modifications and throw an <see cref="InvalidTechSupportProposalTransitionException"/>.
    /// </summary>
    /// <param name="nonPendingStatus">The non-pending lifecycle proposal status to verify.</param>
    /// <param name="matrixTrackLabel">The nullable descriptor helper variable passed explicitly to suppress code diagnostics warning messages.</param>
    [Theory]
    [InlineData(TechSupportProposalStatus.Accepted, "Historically Evaluated and Accepted Proposal Tracking Track")]
    [InlineData(TechSupportProposalStatus.Rejected, "Historically Evaluated and Declined Proposal Tracking Track")]
    public async Task Handle_GivenTechSupportProposalInNonPendingStatus_ShouldPropagateInvalidTechSupportProposalTransitionExceptionAndAbort(
        TechSupportProposalStatus nonPendingStatus,
        string? matrixTrackLabel)
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var targetProposalId = Guid.NewGuid();
        var studentOwnerId = Guid.NewGuid();
        var techSupportAccountId = Guid.NewGuid();

        var command = new ReviewTechSupportProposalCommand
        {
            ProjectInstanceId = targetInstanceId,
            TechSupportProposalId = targetProposalId,
            Accept = true
        };

        var projectInstance = CreateMockProjectInstance(studentOwnerId, ProjectInstanceStatus.Active);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        projectInstance.ProposeTechSupport(techSupportAccountId, DateTime.UtcNow);
        var proposal = projectInstance.TechSupportProposals.First();
        SetEntityPrivateProperty(proposal, nameof(TechSupportProposal.Id), targetProposalId);

        // Force status configuration update out of pending state manually to test transition boundaries
        SetEntityPrivateProperty(proposal, nameof(TechSupportProposal.Status), nonPendingStatus);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(studentOwnerId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        matrixTrackLabel.Should().NotBeNullOrWhiteSpace();
        await act.Should().ThrowAsync<InvalidTechSupportProposalTransitionException>();

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that passing an empty or whitespace string for the rejection reason parameters maps safely to 
    /// fallback string properties within the core aggregate root layout.
    /// </summary>
    /// <param name="emptyOrWhitespaceReason">The empty or whitespace string inputs to pass inside the evaluation query.</param>
    [Theory]
    [InlineData("")]
    [InlineData("    ")]
    [InlineData(null)]
    public async Task Handle_GivenDeclineChoiceWithMissingOrWhitespaceRejectionReason_ShouldFallBackToStandardDefaultMessageString(
        string? emptyOrWhitespaceReason)
    {
        // Arrange
        var targetInstanceId = Guid.NewGuid();
        var targetProposalId = Guid.NewGuid();
        var studentOwnerId = Guid.NewGuid();
        var techSupportAccountId = Guid.NewGuid();

        var command = new ReviewTechSupportProposalCommand
        {
            ProjectInstanceId = targetInstanceId,
            TechSupportProposalId = targetProposalId,
            Accept = false,
            RejectionReason = emptyOrWhitespaceReason
        };

        var projectInstance = CreateMockProjectInstance(studentOwnerId, ProjectInstanceStatus.Active);
        SetEntityPrivateProperty(projectInstance, nameof(ProjectInstance.Id), targetInstanceId);

        projectInstance.ProposeTechSupport(techSupportAccountId, DateTime.UtcNow);
        var proposal = projectInstance.TechSupportProposals.First();
        SetEntityPrivateProperty(proposal, nameof(TechSupportProposal.Id), targetProposalId);

        _mockCurrentUserService.Setup(s => s.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(s => s.UserId).Returns(studentOwnerId);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance> { projectInstance }.BuildMockDbSet().Object);

        // Act
        await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        proposal.Status.Should().Be(TechSupportProposalStatus.Rejected);
        proposal.RejectionReason.Should().Be("Declined by student workspace owner.");

        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
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
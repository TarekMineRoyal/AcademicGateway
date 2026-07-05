using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProjectInstances.Commands.StartProject;
using AcademicGateway.Domain.ProjectInstances;
using AcademicGateway.Domain.ProjectInstances.Enums;
using AcademicGateway.Domain.ProjectInstances.Services;
using AcademicGateway.Domain.ProjectTemplates;
using AcademicGateway.Domain.ProjectTemplates.Enums;
using AcademicGateway.Domain.ProjectTemplates.Exceptions;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.ProjectInstances.Commands.StartProject;

/// <summary>
/// Contains isolated production-grade unit verification tests for the <see cref="StartProjectCommandHandler"/>.
/// Validates blueprint retrieval, factory cloning loops, aggregate status branching, domain exception propagation, 
/// and atomic relational transaction persistence boundaries.
/// </summary>
public class StartProjectCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IDateTimeProvider> _mockDateTimeProvider;
    private readonly StartProjectCommandHandler _handler;
    private readonly DateTime _deterministicUtcNow;

    /// <summary>
    /// Initializes a pristine instance of the <see cref="StartProjectCommandHandlerTests"/> class with isolated tracking mocks.
    /// </summary>
    public StartProjectCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockDateTimeProvider = new Mock<IDateTimeProvider>();

        // 1. Create a concrete instance of the factory class
        var localMilestoneFactory = new LocalMilestoneFactory();

        // 2. Supply the factory instance as the third constructor parameter
        _handler = new StartProjectCommandHandler(_mockContext.Object, _mockDateTimeProvider.Object, localMilestoneFactory);

        _deterministicUtcNow = new DateTime(2026, 7, 3, 12, 0, 0, DateTimeKind.Utc);

        _mockDateTimeProvider.Setup(d => d.UtcNow).Returns(_deterministicUtcNow);
    }

    /// <summary>
    /// Assures that a student initiating a workspace from a valid, approved template without choosing an academic
    /// mentor successfully generates an active runtime project workspace, cloning skills, and committing records to data streams.
    /// </summary>
    [Fact]
    public async Task Handle_GivenValidCommandWithoutProfessor_ShouldCreateActiveProjectInstanceAndCommitToDatabase()
    {
        // Arrange
        var targetTemplateId = Guid.NewGuid();
        var targetStudentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var skillId1 = Guid.NewGuid();
        var skillId2 = Guid.NewGuid();

        var command = new StartProjectCommand
        {
            TemplateId = targetTemplateId,
            StudentId = targetStudentId,
            RequestedProfessorId = null
        };

        // Enforce true encapsulation constructor pipeline to transition to Approved state naturally
        var template = new ProjectTemplate("Distributed Systems Orchestration", "Comprehensive microservices roadmap blueprint.", providerId);
        template.AddSkill(skillId1);
        template.AddSkill(skillId2);
        template.SubmitForReview();
        template.Approve();

        // Adjust entity internal Id tracking to line up with mock request parameters
        typeof(ProjectTemplate).GetProperty(nameof(ProjectTemplate.Id))?.SetValue(template, targetTemplateId);

        _mockContext.Setup(c => c.ProjectTemplates)
            .Returns(new List<ProjectTemplate> { template }.BuildMockDbSet().Object);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance>().BuildMockDbSet().Object);

        // Act
        var resultInstanceId = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        resultInstanceId.Should().NotBeEmpty();

        _mockContext.Verify(c => c.ProjectInstances.Add(It.Is<ProjectInstance>(instance =>
            instance.Id == resultInstanceId &&
            instance.TemplateId == targetTemplateId &&
            instance.StudentId == targetStudentId &&
            instance.ProviderId == providerId &&
            instance.TitleSnapshot == "Distributed Systems Orchestration" &&
            instance.DescriptionSnapshot == "Comprehensive microservices roadmap blueprint." &&
            instance.Status == ProjectInstanceStatus.Active &&
            instance.CreatedAt == _deterministicUtcNow &&
            instance.SupervisorId == null &&
            instance.SnapshotSkills.Count == 2 &&
            instance.SnapshotSkills.Any(s => s.SkillId == skillId1) &&
            instance.SnapshotSkills.Any(s => s.SkillId == skillId2) &&
            instance.SupervisionRequests.Count == 0 &&
            instance.DomainEvents.Count == 1
        )), Times.Once);

        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that when a specific academic supervisor is requested at startup, the instance initializes
    /// into a paused track awaiting matching approval, immediately seeding a pending supervision request.
    /// </summary>
    [Fact]
    public async Task Handle_GivenValidCommandWithRequestedProfessor_ShouldCreateAwaitingSupervisionProjectInstanceWithSeededSupervisionRequest()
    {
        // Arrange
        var targetTemplateId = Guid.NewGuid();
        var targetStudentId = Guid.NewGuid();
        var targetProfessorId = Guid.NewGuid();
        var providerId = Guid.NewGuid();

        var command = new StartProjectCommand
        {
            TemplateId = targetTemplateId,
            StudentId = targetStudentId,
            RequestedProfessorId = targetProfessorId
        };

        var template = new ProjectTemplate("Quantum Computing Algorithms", "Advanced simulation architectures outline.", providerId);
        template.SubmitForReview();
        template.Approve();

        typeof(ProjectTemplate).GetProperty(nameof(ProjectTemplate.Id))?.SetValue(template, targetTemplateId);

        _mockContext.Setup(c => c.ProjectTemplates)
            .Returns(new List<ProjectTemplate> { template }.BuildMockDbSet().Object);

        _mockContext.Setup(c => c.ProjectInstances)
            .Returns(new List<ProjectInstance>().BuildMockDbSet().Object);

        // Act
        var resultInstanceId = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        resultInstanceId.Should().NotBeEmpty();

        _mockContext.Verify(c => c.ProjectInstances.Add(It.Is<ProjectInstance>(instance =>
            instance.Id == resultInstanceId &&
            instance.Status == ProjectInstanceStatus.AwaitingSupervision &&
            instance.SupervisionRequests.Count == 1 &&
            instance.SupervisionRequests.First().ProfessorId == targetProfessorId &&
            instance.SupervisionRequests.First().Status == SupervisionRequestStatus.Pending &&
            instance.SupervisionRequests.First().PitchText.Contains("Initial matchmaking selection")
        )), Times.Once);

        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that requesting a workspace setup from an invalid or non-existent template structural layout key 
    /// completely breaks processing loop routines and propagates a descriptive <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNonExistentTemplateId_ShouldThrowKeyNotFoundExceptionAndAbort()
    {
        // Arrange
        var wrongTemplateId = Guid.NewGuid();
        var command = new StartProjectCommand
        {
            TemplateId = wrongTemplateId,
            StudentId = Guid.NewGuid(),
            RequestedProfessorId = null
        };

        _mockContext.Setup(c => c.ProjectTemplates)
            .Returns(new List<ProjectTemplate>().BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*The requested project template blueprint with ID '{wrongTemplateId}' was not found.*");

        _mockContext.Verify(c => c.ProjectInstances.Add(It.IsAny<ProjectInstance>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that any attempt to initialize a project execution workspace from a non-approved 
    /// blueprint template triggers deep aggregate domain protection, throwing an <see cref="InvalidTemplateStatusException"/>.
    /// </summary>
    /// <param name="invalidStatus">The unapproved blueprint template lifecycle tracking state.</param>
    /// <param name="statusLabel">The nullable diagnosis metadata string passed to explicitly eliminate analyzer warnings.</param>
    [Theory]
    [InlineData(ProjectTemplateStatus.Draft, "Draft State Execution Tracker")]
    [InlineData(ProjectTemplateStatus.PendingReview, "PendingReview State Execution Tracker")]
    [InlineData(ProjectTemplateStatus.ChangesRequested, "ChangesRequested State Execution Tracker")]
    [InlineData(ProjectTemplateStatus.PendingProviderAcceptance, "PendingProviderAcceptance State Execution Tracker")]
    [InlineData(ProjectTemplateStatus.Rejected, "Rejected State Execution Tracker")]
    public async Task Handle_GivenTemplateInNonApprovedStatus_ShouldPropagateInvalidTemplateStatusExceptionAndAbort(
        ProjectTemplateStatus invalidStatus,
        string? statusLabel)
    {
        // Arrange
        var targetTemplateId = Guid.NewGuid();
        var command = new StartProjectCommand
        {
            TemplateId = targetTemplateId,
            StudentId = Guid.NewGuid(),
            RequestedProfessorId = null
        };

        var template = new ProjectTemplate("Improper Configuration Specification", "Will fail pipeline activation criteria checks.", Guid.NewGuid());

        // Advance the status track incrementally based on matrix setup criteria parameters
        if (invalidStatus == ProjectTemplateStatus.PendingReview)
        {
            template.SubmitForReview();
        }
        else if (invalidStatus == ProjectTemplateStatus.ChangesRequested)
        {
            template.SubmitForReview();
            template.RequestChanges("Need minor spelling fixes.");
        }
        else if (invalidStatus == ProjectTemplateStatus.PendingProviderAcceptance)
        {
            template.SubmitForReview();
            template.ProposeReviewerChanges("Adjusted Title", "Adjusted Description");
        }
        else if (invalidStatus == ProjectTemplateStatus.Rejected)
        {
            template.SubmitForReview();
            template.RejectPermanently("Violates programmatic curriculum content rules guidelines.");
        }

        typeof(ProjectTemplate).GetProperty(nameof(ProjectTemplate.Id))?.SetValue(template, targetTemplateId);

        _mockContext.Setup(c => c.ProjectTemplates)
            .Returns(new List<ProjectTemplate> { template }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        statusLabel.Should().NotBeNullOrWhiteSpace();
        await act.Should().ThrowAsync<InvalidTemplateStatusException>();

        _mockContext.Verify(c => c.ProjectInstances.Add(It.IsAny<ProjectInstance>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if the initiating student context translates an empty tracking layout key, 
    /// aggregate validation rules trigger immediately and throw an <see cref="InvalidTemplateDetailsException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenEmptyStudentId_ShouldPropagateInvalidTemplateDetailsExceptionAndAbort()
    {
        // Arrange
        var targetTemplateId = Guid.NewGuid();
        var command = new StartProjectCommand
        {
            TemplateId = targetTemplateId,
            StudentId = Guid.Empty, // Violates identity tracking aggregate rules rules
            RequestedProfessorId = null
        };

        var template = new ProjectTemplate("Secure E-Commerce Gateway", "Payment infrastructure blueprint description layout.", Guid.NewGuid());
        template.SubmitForReview();
        template.Approve();

        typeof(ProjectTemplate).GetProperty(nameof(ProjectTemplate.Id))?.SetValue(template, targetTemplateId);

        _mockContext.Setup(c => c.ProjectTemplates)
            .Returns(new List<ProjectTemplate> { template }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidTemplateDetailsException>()
            .WithMessage("*Student ID cannot be an empty Guid when instantiating a project.*");

        _mockContext.Verify(c => c.ProjectInstances.Add(It.IsAny<ProjectInstance>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProjectTemplates.Commands.CreateProjectTemplate;
using AcademicGateway.Domain.ProjectTemplates;
using AcademicGateway.Domain.ProjectTemplates.Enums;
using AcademicGateway.Domain.ProjectTemplates.Exceptions;
using AcademicGateway.Domain.Providers;
using AcademicGateway.Domain.Providers.Exceptions;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.ProjectTemplates.Commands.CreateProjectTemplate;

/// <summary>
/// Contains isolated unit verification routines for the <see cref="CreateProjectTemplateCommandHandler"/>.
/// Validates provider verification rules, aggregate initialization invariants, lifecycle transitions,
/// collection boundary edge cases, and skill matrix structural limits.
/// </summary>
public class CreateProjectTemplateCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly CreateProjectTemplateCommandHandler _handler;

    /// <summary>
    /// Initializes a pristine instance of the test class with isolated database mock mappings.
    /// </summary>
    public CreateProjectTemplateCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new CreateProjectTemplateCommandHandler(_mockContext.Object);
    }

    /// <summary>
    /// Assures that a formally verified industry partner providing a valid title, description, and list of skill 
    /// tracking identifiers successfully creates a blueprint template transitioned into a review pipeline state.
    /// </summary>
    [Fact]
    public async Task Handle_GivenVerifiedProviderWithValidSkills_ShouldCreateTemplateAndQueueLifecycleReview()
    {
        // Arrange
        var targetProviderId = Guid.NewGuid();
        var skillId1 = Guid.NewGuid();
        var skillId2 = Guid.NewGuid();

        var command = new CreateProjectTemplateCommand
        {
            ProviderId = targetProviderId,
            Title = "Advanced Cloud Architecture Setup",
            Description = "An intensive multi-week project focusing on high-availability distributed infrastructure deployments.",
            SkillIds = new List<Guid> { skillId1, skillId2 }
        };

        var provider = new Provider(targetProviderId, "Acme Cloud Solutions");
        provider.VerifyProfile(); // Elevates permissibility status to allow template publishing

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.ProjectTemplates).Returns(new List<ProjectTemplate>().BuildMockDbSet().Object);

        // Act
        var resultTemplateId = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        resultTemplateId.Should().NotBeEmpty();

        _mockContext.Verify(c => c.ProjectTemplates.Add(It.Is<ProjectTemplate>(t =>
            t.Id == resultTemplateId &&
            t.ProviderId == targetProviderId &&
            t.Title == command.Title &&
            t.Description == command.Description &&
            t.Status == ProjectTemplateStatus.PendingReview &&
            t.ProjectTemplateSkills.Count == 2 &&
            t.ProjectTemplateSkills.Any(pts => pts.SkillId == skillId1) &&
            t.ProjectTemplateSkills.Any(pts => pts.SkillId == skillId2)
        )), Times.Once);

        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that when <see cref="CreateProjectTemplateCommand.SkillIds"/> is passed as null,
    /// the handler handles the conditional gracefully, bypassing skill processing loops entirely 
    /// without throwing a NullReferenceException, and saves the entity with zero skills.
    /// </summary>
    [Fact]
    public async Task Handle_GivenVerifiedProviderWithNullSkillsCollection_ShouldCreateTemplateSafelyWithZeroSkills()
    {
        // Arrange
        var targetProviderId = Guid.NewGuid();
        var command = new CreateProjectTemplateCommand
        {
            ProviderId = targetProviderId,
            Title = "Data Analytics Processing Blueprint",
            Description = "A point-in-time snapshot processing engine blueprint track setup configuration.",
            SkillIds = null! // Fixed CS8625: Explicitly passing null! to force unit coverage of defensive handler loop guard
        };

        var provider = new Provider(targetProviderId, "Global Analytics Corp");
        provider.VerifyProfile();

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.ProjectTemplates).Returns(new List<ProjectTemplate>().BuildMockDbSet().Object);

        // Act
        var resultTemplateId = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        resultTemplateId.Should().NotBeEmpty();

        _mockContext.Verify(c => c.ProjectTemplates.Add(It.Is<ProjectTemplate>(t =>
            t.Id == resultTemplateId &&
            t.ProjectTemplateSkills.Count == 0
        )), Times.Once);

        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that when an empty list of skill tracking identifiers is supplied, the processing 
    /// sequence completes flawlessly, establishing a template instance housing 0 skills.
    /// </summary>
    [Fact]
    public async Task Handle_GivenVerifiedProviderWithEmptySkillsCollection_ShouldCreateTemplateSafelyWithZeroSkills()
    {
        // Arrange
        var targetProviderId = Guid.NewGuid();
        var command = new CreateProjectTemplateCommand
        {
            ProviderId = targetProviderId,
            Title = "Automated Systems Integration Outline",
            Description = "An architectural structural layout planning out continuous execution models.",
            SkillIds = new List<Guid>() // Empty list branch condition
        };

        var provider = new Provider(targetProviderId, "Automation Labs Inc");
        provider.VerifyProfile();

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.ProjectTemplates).Returns(new List<ProjectTemplate>().BuildMockDbSet().Object);

        // Act
        var resultTemplateId = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        resultTemplateId.Should().NotBeEmpty();

        _mockContext.Verify(c => c.ProjectTemplates.Add(It.Is<ProjectTemplate>(t =>
            t.Id == resultTemplateId &&
            t.ProjectTemplateSkills.Count == 0
        )), Times.Once);

        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that supplying an empty or non-existent provider identity tracker reference context
    /// immediately disrupts processing and throws a descriptive <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNonExistentProviderId_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var wrongProviderId = Guid.NewGuid();
        var command = new CreateProjectTemplateCommand
        {
            ProviderId = wrongProviderId,
            Title = "Valid Curriculum Design Track Summary",
            Description = "This description fulfills length validation requirements minimum parameters.",
            SkillIds = Array.Empty<Guid>()
        };

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider>().BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*Provider profile with ID '{wrongProviderId}' was not found.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that an unverified corporate provider attempting to issue a project template blueprint
    /// transitions into a hard denial path and triggers a <see cref="ProviderNotVerifiedException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenUnverifiedProvider_ShouldThrowProviderNotVerifiedExceptionAndAbortPersistence()
    {
        // Arrange
        var targetProviderId = Guid.NewGuid();
        var command = new CreateProjectTemplateCommand
        {
            ProviderId = targetProviderId,
            Title = "Unauthorized Content Layout Draft",
            Description = "This description fulfills length validation requirements minimum parameters.",
            SkillIds = Array.Empty<Guid>()
        };

        var provider = new Provider(targetProviderId, "Unverified Corp");

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<ProviderNotVerifiedException>();

        _mockContext.Verify(c => c.ProjectTemplates.Add(It.IsAny<ProjectTemplate>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if an empty or whitespace string is submitted as the title parameter, the underlying 
    /// domain aggregate encapsulation block rejects instantiation, bubbling up an <see cref="InvalidTemplateDetailsException"/>.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Handle_GivenInvalidTitle_ShouldPropagateInvalidTemplateDetailsExceptionAndAbort(string? invalidTitle)
    {
        // Arrange
        var targetProviderId = Guid.NewGuid();
        var command = new CreateProjectTemplateCommand
        {
            ProviderId = targetProviderId,
            Title = invalidTitle!,
            Description = "Valid structural description text content summarizing workflow objectives.",
            SkillIds = Array.Empty<Guid>()
        };

        var provider = new Provider(targetProviderId, "Validated Platform Partner");
        provider.VerifyProfile();

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidTemplateDetailsException>()
            .WithMessage("*Project template title cannot be empty or whitespace.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if an empty or whitespace string is submitted as the description parameter, the underlying 
    /// domain aggregate encapsulation block rejects instantiation, bubbling up an <see cref="InvalidTemplateDetailsException"/>.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" \t \n ")]
    [InlineData(null)]
    public async Task Handle_GivenInvalidDescription_ShouldPropagateInvalidTemplateDetailsExceptionAndAbort(string? invalidDesc)
    {
        // Arrange
        var targetProviderId = Guid.NewGuid();
        var command = new CreateProjectTemplateCommand
        {
            ProviderId = targetProviderId,
            Title = "Valid Integration Framework Title",
            Description = invalidDesc!,
            SkillIds = Array.Empty<Guid>()
        };

        var provider = new Provider(targetProviderId, "Validated Platform Partner");
        provider.VerifyProfile();

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidTemplateDetailsException>()
            .WithMessage("*Project template description cannot be empty or whitespace.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that passing an empty tracking Guid inside the Skill list collection invokes deep domain protection guards, 
    /// breaking execution and raising an <see cref="InvalidTemplateDetailsException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenEmptyGuidSkillId_ShouldPropagateInvalidTemplateDetailsExceptionAndAbort()
    {
        // Arrange
        var targetProviderId = Guid.NewGuid();
        var command = new CreateProjectTemplateCommand
        {
            ProviderId = targetProviderId,
            Title = "Systems Reliability Diagnostics",
            Description = "An advanced curriculum analyzing system failover and infrastructure monitoring pipelines.",
            SkillIds = new List<Guid> { Guid.NewGuid(), Guid.Empty } // House an invalid tracker
        };

        var provider = new Provider(targetProviderId, "High-Availability Solutions");
        provider.VerifyProfile();

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidTemplateDetailsException>()
            .WithMessage("*Skill ID cannot be an empty Guid.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if a command contains more than 10 skills, the internal domain entity collections 
    /// reject the structural mutation layout rules, throwing an <see cref="InvalidTemplateDetailsException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenMoreThanTenSkills_ShouldPropagateInvalidTemplateDetailsExceptionAndAbort()
    {
        // Arrange
        var targetProviderId = Guid.NewGuid();

        // Populate a collection with 11 discrete, non-empty Guids to break the maximum limit threshold rule
        var bloatedSkillIds = Enumerable.Range(1, 11).Select(_ => Guid.NewGuid()).ToList();

        var command = new CreateProjectTemplateCommand
        {
            ProviderId = targetProviderId,
            Title = "Hyper-Disciplined Development Track",
            Description = "A curriculum trying to track far too many technological metrics concurrently.",
            SkillIds = bloatedSkillIds
        };

        var provider = new Provider(targetProviderId, "Bloated Matrix Tech Corp");
        provider.VerifyProfile();

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidTemplateDetailsException>()
            .WithMessage("*A single project template cannot require more than 10 technical skills.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
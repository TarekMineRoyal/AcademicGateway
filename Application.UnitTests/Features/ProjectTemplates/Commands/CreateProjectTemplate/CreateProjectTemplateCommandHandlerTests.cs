using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProjectTemplates.Commands.CreateProjectTemplate;
using AcademicGateway.Domain.ProjectTemplates;
using AcademicGateway.Domain.ProjectTemplates.Enums;
using AcademicGateway.Domain.Providers;
using AcademicGateway.Domain.Providers.Exceptions;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.ProjectTemplates.Commands.CreateProjectTemplate;

/// <summary>
/// Contains isolated unit verification routines for the <see cref="CreateProjectTemplateCommandHandler"/>.
/// Validates provider verification rules, aggregate initialization invariants, lifecycle transitions, and skill matrix indexing.
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

        // Best Practice: Instantiate domain entities natively to respect core business validation behavior
        var provider = new Provider(targetProviderId, "Acme Cloud Solutions");
        provider.VerifyProfile(); // Elevates permissibility status to allow template publishing

        // Establish the mocked relational tracking collection contexts
        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.ProjectTemplates).Returns(new List<ProjectTemplate>().BuildMockDbSet().Object);

        // Act
        // Best Practice (xUnit1051): Pass TestContext.Current.CancellationToken for responsive test run cancellations.
        var resultTemplateId = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        resultTemplateId.Should().NotBeEmpty();

        // Verify the project template itself was tracked, hydrated with correct variables, and pushed to PendingReview status
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

        // Confirm transactional persistence integrity across the unit of work boundaries
        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
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

        // Onboarding gate mismatch context: IsVerified is left to default FALSE standing
        var provider = new Provider(targetProviderId, "Unverified Corp");

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<ProviderNotVerifiedException>();

        // Guarantee that no storage transactions or tracking alterations leaked into the unit of work
        _mockContext.Verify(c => c.ProjectTemplates.Add(It.IsAny<ProjectTemplate>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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
}
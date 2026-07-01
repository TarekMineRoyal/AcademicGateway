using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProjectTemplates.Commands.CreateTemplate;
using Domain.Lookups;
using Domain.ProjectTemplates;
using Domain.ProjectTemplates.Enums;
using Domain.Providers;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.ProjectTemplates.Commands.CreateTemplate;

public class CreateProjectTemplateCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly CreateProjectTemplateCommandHandler _handler;

    public CreateProjectTemplateCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new CreateProjectTemplateCommandHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_VerifiedProviderWithValidSkills_ShouldCreateTemplateAndLinkSkills()
    {
        // Arrange
        var targetProviderId = "auth0|verified_provider_123";
        var skillId1 = Guid.NewGuid();
        var skillId2 = Guid.NewGuid();

        var command = new CreateProjectTemplateCommand
        {
            ProviderId = targetProviderId,
            Title = "Advanced Cloud Architecture Setup",
            Description = "A intensive multi-week project focusing on high-availability distributed infrastructure deployments.",
            ExpectedDurationWeeks = 8,
            SkillIds = new List<Guid> { skillId1, skillId2 }
        };

        // Instantiate verified provider entity
        var provider = CreateEntityWithPrivateConstructor<Provider>();
        SetPrivateProperty(provider, nameof(Provider.UserId), targetProviderId);
        SetPrivateProperty(provider, nameof(Provider.IsVerified), true);

        // Instantiate existing platform lookups for the skills
        var skill1 = CreateEntityWithPrivateConstructor<Skill>();
        SetPrivateProperty(skill1, nameof(Skill.Id), skillId1);
        SetPrivateProperty(skill1, nameof(Skill.Name), "AWS CloudFormation");

        var skill2 = CreateEntityWithPrivateConstructor<Skill>();
        SetPrivateProperty(skill2, nameof(Skill.Id), skillId2);
        SetPrivateProperty(skill2, nameof(Skill.Name), "Terraform IaaC");

        // Set up the mocked database sets
        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.Skills).Returns(new List<Skill> { skill1, skill2 }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.ProjectTemplates).Returns(new List<ProjectTemplate>().BuildMockDbSet().Object);
        _mockContext.Setup(c => c.ProjectTemplateSkills).Returns(new List<ProjectTemplateSkill>().BuildMockDbSet().Object);

        // Act
        var resultTemplateId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultTemplateId.Should().NotBeEmpty();

        // Verify the project template itself was tracked and pushed to review state
        _mockContext.Verify(c => c.ProjectTemplates.Add(It.Is<ProjectTemplate>(t =>
            t.ProviderId == targetProviderId &&
            t.Title == command.Title &&
            t.Status == ProjectTemplateStatus.PendingReview
        )), Times.Once);

        // Verify that BOTH relational join rows were mapped and attached to the tracking context
        _mockContext.Verify(c => c.ProjectTemplateSkills.Add(It.Is<ProjectTemplateSkill>(pts => pts.SkillId == skillId1)), Times.Once);
        _mockContext.Verify(c => c.ProjectTemplateSkills.Add(It.Is<ProjectTemplateSkill>(pts => pts.SkillId == skillId2)), Times.Once);

        // Confirm database transaction integrity
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UnverifiedProvider_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var targetProviderId = "auth0|unverified_cheater";
        var command = new CreateProjectTemplateCommand
        {
            ProviderId = targetProviderId,
            Title = "Malicious or Unauthorized Content Draft",
            Description = "This description fulfills length validation requirements minimum parameters.",
            ExpectedDurationWeeks = 4,
            SkillIds = new List<Guid>()
        };

        // Security Gate breach condition: IsVerified is hardcoded to FALSE
        var provider = CreateEntityWithPrivateConstructor<Provider>();
        SetPrivateProperty(provider, nameof(Provider.UserId), targetProviderId);
        SetPrivateProperty(provider, nameof(Provider.IsVerified), false);

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Unverified providers are restricted from creating project templates*");

        // Guarantee that no tracking alterations bypassed memory checks to touch persistence layers
        _mockContext.Verify(c => c.ProjectTemplates.Add(It.IsAny<ProjectTemplate>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ProviderNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var command = new CreateProjectTemplateCommand
        {
            ProviderId = "auth0|ghost_identity",
            Title = "Valid Curriculum Design Track Summary",
            Description = "This description fulfills length validation requirements minimum parameters.",
            ExpectedDurationWeeks = 6,
            SkillIds = new List<Guid>()
        };

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider>().BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*was not found*");
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static T CreateEntityWithPrivateConstructor<T>() =>
        (T)Activator.CreateInstance(typeof(T), true)!;

    private static void SetPrivateProperty(object obj, string propertyName, object value) =>
        obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?
           .SetValue(obj, value);
}
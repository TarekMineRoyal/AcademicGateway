using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProjectTemplates.Commands.CreateProjectTemplate;
using AcademicGateway.Domain.Skills;
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
/// Contains highly isolated unit validation routines for the <see cref="CreateProjectTemplateCommandValidator"/>.
/// Evaluates text boundary conditions, token identification fields, collection capacity ceilings, 
/// and asynchronous database dependency criteria.
/// </summary>
public class CreateProjectTemplateCommandValidatorTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly CreateProjectTemplateCommandValidator _validator;

    /// <summary>
    /// Initializes a pristine instance of the validator test framework with isolated context mocks.
    /// </summary>
    public CreateProjectTemplateCommandValidatorTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _validator = new CreateProjectTemplateCommandValidator(_mockContext.Object);
    }

    /// <summary>
    /// Assures that a perfectly formatted command context containing valid lengths and confirmed 
    /// persistent tracking references successfully clears the architectural validation layers.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_GivenPerfectCommandArguments_ShouldPassValidationCleanly()
    {
        // Arrange
        var targetSkill1 = new Skill("Cloud Engineering");
        var targetSkill2 = new Skill("Distributed Pipelines");
        var skillsData = new List<Skill> { targetSkill1, targetSkill2 };

        _mockContext.Setup(c => c.Skills)
            .Returns(skillsData.BuildMockDbSet().Object);

        var command = new CreateProjectTemplateCommand
        {
            ProviderId = Guid.NewGuid(),
            Title = "Enterprise Cloud Deployments",
            Description = "An absolute deep-dive project designed to architect highly available cloud setups.",
            SkillIds = new List<Guid> { targetSkill1.Id, targetSkill2.Id }
        };

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// Assures that an uninitialized empty system identity tracking reference context explicitly 
    /// triggers a structure validation breach event framework.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_GivenEmptyProviderId_ShouldFailWithRequiredMessage()
    {
        // Arrange
        _mockContext.Setup(c => c.Skills).Returns(new List<Skill>().BuildMockDbSet().Object);

        var command = new CreateProjectTemplateCommand
        {
            ProviderId = Guid.Empty,
            Title = "Valid Project Title Blueprint",
            Description = "A valid structural description text containing more than twenty characters long requirements.",
            SkillIds = new List<Guid> { Guid.NewGuid() }
        };

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(CreateProjectTemplateCommand.ProviderId) &&
            e.ErrorMessage == "Provider ID is required.");
    }

    /// <summary>
    /// Assures that invalid, null, empty, or whitespace title variants break execution constraints immediately.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ValidateAsync_GivenNullOrEmptyTitle_ShouldFailWithRequiredMessage(string? invalidTitle)
    {
        // Arrange
        _mockContext.Setup(c => c.Skills).Returns(new List<Skill>().BuildMockDbSet().Object);

        var command = new CreateProjectTemplateCommand
        {
            ProviderId = Guid.NewGuid(),
            Title = invalidTitle!,
            Description = "A valid structural description text containing more than twenty characters long requirements.",
            SkillIds = new List<Guid> { Guid.NewGuid() }
        };

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreateProjectTemplateCommand.Title) &&
            e.ErrorMessage == "Project title is required.");
    }

    /// <summary>
    /// Assures that titles violating the lower-bound structural character parameter rules yield descriptive validation faults.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_GivenTooShortTitle_ShouldFailWithMinimumLengthMessage()
    {
        // Arrange
        _mockContext.Setup(c => c.Skills).Returns(new List<Skill>().BuildMockDbSet().Object);

        var command = new CreateProjectTemplateCommand
        {
            ProviderId = Guid.NewGuid(),
            Title = "Abcd", // 4 characters - short of 5 min length limit
            Description = "A valid structural description text containing more than twenty characters long requirements.",
            SkillIds = new List<Guid> { Guid.NewGuid() }
        };

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(CreateProjectTemplateCommand.Title) &&
            e.ErrorMessage == "Project title must be at least 5 characters long.");
    }

    /// <summary>
    /// Assures that a title tracking exactly at the upper structural limit constraint clears rules perfectly, 
    /// while exceeding it by one character invokes a field validation denial block.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_TitleLengthBoundaryChecks_ShouldRespectUpperCapsStrictly()
    {
        // Arrange
        var baseSkill = new Skill("Software Craftsmanship");
        _mockContext.Setup(c => c.Skills).Returns(new List<Skill> { baseSkill }.BuildMockDbSet().Object);

        var validTitleExactlyMax = new string('A', 100);
        var invalidTitleOverflow = new string('B', 101);

        var validCommand = new CreateProjectTemplateCommand
        {
            ProviderId = Guid.NewGuid(),
            Title = validTitleExactlyMax,
            Description = "A valid structural description text containing more than twenty characters long requirements.",
            SkillIds = new List<Guid> { baseSkill.Id }
        };

        var invalidCommand = new CreateProjectTemplateCommand
        {
            ProviderId = Guid.NewGuid(),
            Title = invalidTitleOverflow,
            Description = "A valid structural description text containing more than twenty characters long requirements.",
            SkillIds = new List<Guid> { baseSkill.Id }
        };

        // Act
        var validResult = await _validator.ValidateAsync(validCommand, TestContext.Current.CancellationToken);
        var invalidResult = await _validator.ValidateAsync(invalidCommand, TestContext.Current.CancellationToken);

        // Assert
        validResult.IsValid.Should().BeTrue();

        invalidResult.IsValid.Should().BeFalse();
        invalidResult.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(CreateProjectTemplateCommand.Title) &&
            e.ErrorMessage == "Project title cannot exceed 100 characters.");
    }

    /// <summary>
    /// Assures that unpopulated or empty text blocks allocated within descriptions result in execution failure diagnostics.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ValidateAsync_GivenNullOrEmptyDescription_ShouldFailWithRequiredMessage(string? invalidDesc)
    {
        // Arrange
        _mockContext.Setup(c => c.Skills).Returns(new List<Skill>().BuildMockDbSet().Object);

        var command = new CreateProjectTemplateCommand
        {
            ProviderId = Guid.NewGuid(),
            Title = "Valid Core Project Title",
            Description = invalidDesc!,
            SkillIds = new List<Guid> { Guid.NewGuid() }
        };

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreateProjectTemplateCommand.Description) &&
            e.ErrorMessage == "Project description is required.");
    }

    /// <summary>
    /// Assures that detailed descriptions falling short of the required 20-character baseline threshold trigger validation rules.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_GivenTooShortDescription_ShouldFailWithMinimumLengthMessage()
    {
        // Arrange
        _mockContext.Setup(c => c.Skills).Returns(new List<Skill>().BuildMockDbSet().Object);

        var command = new CreateProjectTemplateCommand
        {
            ProviderId = Guid.NewGuid(),
            Title = "Valid System Title",
            Description = "Short description.", // 18 characters - short of 20 min threshold
            SkillIds = new List<Guid> { Guid.NewGuid() }
        };

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(CreateProjectTemplateCommand.Description) &&
            e.ErrorMessage == "Project description must provide at least 20 characters of detail.");
    }

    /// <summary>
    /// Assures that project outlines containing description entries exceeding the maximum upper limit of 2000 characters 
    /// are caught cleanly by system guards.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_GivenTooLongDescription_ShouldFailWithMaximumLengthMessage()
    {
        // Arrange
        _mockContext.Setup(c => c.Skills).Returns(new List<Skill>().BuildMockDbSet().Object);

        var command = new CreateProjectTemplateCommand
        {
            ProviderId = Guid.NewGuid(),
            Title = "Valid Systems Integration Architecture",
            Description = new string('D', 2001), // Exceeds upper structural boundary criteria limits
            SkillIds = new List<Guid> { Guid.NewGuid() }
        };

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(CreateProjectTemplateCommand.Description) &&
            e.ErrorMessage == "Project description cannot exceed 2000 characters.");
    }

    /// <summary>
    /// Assures that providing an empty collection or null sequence configuration details returns explicit structural faults.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_GivenEmptySkillIdsCollection_ShouldFailWithNotEmptyMessage()
    {
        // Arrange
        _mockContext.Setup(c => c.Skills).Returns(new List<Skill>().BuildMockDbSet().Object);

        var command = new CreateProjectTemplateCommand
        {
            ProviderId = Guid.NewGuid(),
            Title = "Valid Framework Engineering Track",
            Description = "This description matches required sizing structures perfectly.",
            SkillIds = Array.Empty<Guid>()
        };

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(CreateProjectTemplateCommand.SkillIds) &&
            e.ErrorMessage == "At least one required skill must be specified for the project template.");
    }

    /// <summary>
    /// Assures that attempting to tie more than 10 technical competency matrix references onto a single 
    /// architecture context breaks internal capacity limits.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_GivenMoreThanTenSkills_ShouldFailWithBloatedCapacityMatrixMessage()
    {
        // Arrange
        _mockContext.Setup(c => c.Skills).Returns(new List<Skill>().BuildMockDbSet().Object);

        var bloatedSkillIdsList = Enumerable.Range(1, 11).Select(_ => Guid.NewGuid()).ToList();

        var command = new CreateProjectTemplateCommand
        {
            ProviderId = Guid.NewGuid(),
            Title = "Hyper Bloated Metric Collection Outline",
            Description = "This description matches required sizing structures perfectly.",
            SkillIds = bloatedSkillIdsList
        };

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreateProjectTemplateCommand.SkillIds) &&
            e.ErrorMessage == "You cannot assign more than 10 required skills to a single template.");
    }

    /// <summary>
    /// Assures that if lookups contain reference identifiers missing from active database directory indices, 
    /// the validation operation captures it and flags a mismatch failure framework.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_GivenSkillsMissingFromDatabaseIndex_ShouldFailWithDirectoryLookupMessage()
    {
        // Arrange
        var realSkill = new Skill("Domain Driven Design");
        var activeDirectory = new List<Skill> { realSkill };

        _mockContext.Setup(c => c.Skills)
            .Returns(activeDirectory.BuildMockDbSet().Object);

        var command = new CreateProjectTemplateCommand
        {
            ProviderId = Guid.NewGuid(),
            Title = "Advanced System Blueprints",
            Description = "This description matches required sizing structures perfectly.",
            SkillIds = new List<Guid> { realSkill.Id, Guid.NewGuid() } // Contains 1 unindexed tracker item
        };

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(CreateProjectTemplateCommand.SkillIds) &&
            e.ErrorMessage == "One or more selected skills do not exist within the system directory.");
    }
}
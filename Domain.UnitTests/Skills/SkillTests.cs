using System;
using AcademicGateway.Domain.Skills;
using AcademicGateway.Domain.Skills.Exceptions;
using FluentAssertions;
using Xunit;

namespace AcademicGateway.Domain.UnitTests.Skills;

public class SkillTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeCorrectly_WhenNameIsValid()
    {
        // Arrange
        var untrimmedName = "   Machine Learning   ";
        var expectedName = "Machine Learning";

        // Act
        var skill = new Skill(untrimmedName);

        // Assert
        skill.Id.Should().NotBeEmpty();
        skill.Name.Should().Be(expectedName);
        skill.StudentSkills.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowEmptySkillNameException_WhenNameIsNullOrEmptyOrWhitespace(string? invalidName)
    {
        // Act
        Action act = () => _ = new Skill(invalidName!);

        // Assert
        act.Should().Throw<EmptySkillNameException>();
    }

    #endregion

    #region UpdateName Tests

    [Fact]
    public void UpdateName_ShouldModifyNameAndTrim_WhenNewNameIsValid()
    {
        // Arrange
        var skill = new Skill("Docker");
        var newValidName = "  Docker Containers  ";
        var expectedName = "Docker Containers";

        // Act
        skill.UpdateName(newValidName);

        // Assert
        skill.Name.Should().Be(expectedName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateName_ShouldThrowEmptySkillNameException_WhenNewNameIsInvalid(string? invalidName)
    {
        // Arrange
        var skill = new Skill("Kubernetes");

        // Act
        Action act = () => skill.UpdateName(invalidName!);

        // Assert
        act.Should().Throw<EmptySkillNameException>();
    }

    #endregion
}
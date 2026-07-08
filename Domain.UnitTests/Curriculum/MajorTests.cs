using System;
using System.Linq;
using AcademicGateway.Domain.Curriculum;
using AcademicGateway.Domain.Curriculum.Exceptions;
using FluentAssertions;
using Xunit;

namespace AcademicGateway.Domain.UnitTests.Curriculum;

public class MajorTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeCorrectly_WhenNameIsValid()
    {
        // Arrange
        var validName = "   Computer Science   ";
        var expectedName = "Computer Science";

        // Act
        var major = new Major(validName);

        // Assert
        major.Id.Should().NotBeEmpty();
        major.Name.Should().Be(expectedName);
        major.Specialties.Should().BeEmpty();
        major.StudentMajors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowEmptyMajorNameException_WhenNameIsNullOrEmptyOrWhitespace(string? invalidName)
    {
        // Act
        Action act = () => _ = new Major(invalidName!);

        // Assert
        act.Should().Throw<EmptyMajorNameException>();
    }

    #endregion

    #region UpdateName Tests

    [Fact]
    public void UpdateName_ShouldModifyNameAndTrim_WhenNewNameIsValid()
    {
        // Arrange
        var major = new Major("Information Systems");
        var newValidName = "  Advanced Information Systems  ";
        var expectedName = "Advanced Information Systems";

        // Act
        major.UpdateName(newValidName);

        // Assert
        major.Name.Should().Be(expectedName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateName_ShouldThrowEmptyMajorNameException_WhenNewNameIsInvalid(string? invalidName)
    {
        // Arrange
        var major = new Major("Mechanical Engineering");

        // Act
        Action act = () => major.UpdateName(invalidName!);

        // Assert
        act.Should().Throw<EmptyMajorNameException>();
    }

    #endregion

    #region AddSpecialty Tests

    [Fact]
    public void AddSpecialty_ShouldAddNewSpecialty_WhenNameIsUnique()
    {
        // Arrange
        var major = new Major("Computer Science");
        var specialtyName = "Artificial Intelligence";

        // Act
        major.AddSpecialty(specialtyName);

        // Assert
        major.Specialties.Should().HaveCount(1);
        var addedSpecialty = major.Specialties.First();
        addedSpecialty.Name.Should().Be(specialtyName);
        addedSpecialty.MajorId.Should().Be(major.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddSpecialty_ShouldThrowEmptySpecialtyNameException_WhenNameIsInvalid(string? invalidName)
    {
        // Arrange
        var major = new Major("Computer Science");

        // Act
        Action act = () => major.AddSpecialty(invalidName!);

        // Assert
        act.Should().Throw<EmptySpecialtyNameException>();
    }

    [Fact]
    public void AddSpecialty_ShouldNotAddDuplicate_WhenNameMatchesExactly()
    {
        // Arrange
        var major = new Major("Computer Science");
        var specialtyName = "Cybersecurity";
        major.AddSpecialty(specialtyName);

        // Act
        major.AddSpecialty(specialtyName);

        // Assert
        major.Specialties.Should().HaveCount(1);
    }

    [Fact]
    public void AddSpecialty_ShouldNotAddDuplicate_WhenNameMatchesCaseInsensitively()
    {
        // Arrange
        var major = new Major("Computer Science");
        major.AddSpecialty("Data Science");

        // Act
        major.AddSpecialty("data science");

        // Assert
        major.Specialties.Should().HaveCount(1);
    }

    #endregion

    #region RemoveSpecialty Tests

    [Fact]
    public void RemoveSpecialty_ShouldEvictSpecialty_WhenSpecialtyExists()
    {
        // Arrange
        var major = new Major("Computer Science");
        var specialtyName = "Cloud Computing";
        major.AddSpecialty(specialtyName);
        var addedSpecialtyId = major.Specialties.First().Id;

        // Act
        major.RemoveSpecialty(addedSpecialtyId);

        // Assert
        major.Specialties.Should().BeEmpty();
    }

    [Fact]
    public void RemoveSpecialty_ShouldDoNothing_WhenSpecialtyDoesNotExist()
    {
        // Arrange
        var major = new Major("Computer Science");
        major.AddSpecialty("Game Development");
        var nonExistentId = Guid.NewGuid();

        // Act
        major.RemoveSpecialty(nonExistentId);

        // Assert
        major.Specialties.Should().HaveCount(1);
    }

    #endregion
}
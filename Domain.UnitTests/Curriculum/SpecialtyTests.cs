using System;
using System.Reflection;
using AcademicGateway.Domain.Curriculum;
using AcademicGateway.Domain.Curriculum.Exceptions;
using FluentAssertions;
using Xunit;

namespace AcademicGateway.Domain.UnitTests.Curriculum;

public class SpecialtyTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeCorrectly_WhenParametersAreValid()
    {
        // Arrange
        var validName = "   Cybersecurity   ";
        var expectedName = "Cybersecurity";
        var parentMajorId = Guid.NewGuid();

        // Act
        var specialty = CreateSpecialtyViaReflection(validName, parentMajorId);

        // Assert
        specialty.Id.Should().NotBeEmpty();
        specialty.Name.Should().Be(expectedName);
        specialty.MajorId.Should().Be(parentMajorId);
        specialty.StudentSpecialties.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidParentMajorIdException_WhenMajorIdIsEmpty()
    {
        // Arrange
        var validName = "Data Science";
        var emptyMajorId = Guid.Empty;

        // Act
        Action act = () => CreateSpecialtyViaReflection(validName, emptyMajorId);

        // Assert
        act.Should().Throw<InvalidParentMajorIdException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowEmptySpecialtyNameException_WhenNameIsNullOrEmptyOrWhitespace(string? invalidName)
    {
        // Arrange
        var parentMajorId = Guid.NewGuid();

        // Act
        Action act = () => CreateSpecialtyViaReflection(invalidName!, parentMajorId);

        // Assert
        act.Should().Throw<EmptySpecialtyNameException>();
    }

    #endregion

    #region UpdateName Tests

    [Fact]
    public void UpdateName_ShouldModifyNameAndTrim_WhenNewNameIsValid()
    {
        // Arrange
        var parentMajorId = Guid.NewGuid();
        var specialty = CreateSpecialtyViaReflection("Software Engineering", parentMajorId);
        var newValidName = "  Advanced Software Engineering  ";
        var expectedName = "Advanced Software Engineering";

        // Act
        specialty.UpdateName(newValidName);

        // Assert
        specialty.Name.Should().Be(expectedName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateName_ShouldThrowEmptySpecialtyNameException_WhenNewNameIsInvalid(string? invalidName)
    {
        // Arrange
        var parentMajorId = Guid.NewGuid();
        var specialty = CreateSpecialtyViaReflection("Cloud Computing", parentMajorId);

        // Act
        Action act = () => specialty.UpdateName(invalidName!);

        // Assert
        act.Should().Throw<EmptySpecialtyNameException>();
    }

    #endregion

    #region Reflection Factory Helper

    /// <summary>
    /// Safely instantiates an instance of <see cref="Specialty"/> by invoking its internal constructor.
    /// This bypasses protection access restrictions (CS0122) cleanly and unwraps targeted DomainExceptions.
    /// </summary>
    private static Specialty CreateSpecialtyViaReflection(string name, Guid majorId)
    {
        var constructor = typeof(Specialty).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            new[] { typeof(string), typeof(Guid) },
            null);

        if (constructor == null)
        {
            throw new InvalidOperationException("Could not locate the internal constructor matching (string, Guid) signature on Specialty.");
        }

        try
        {
            return (Specialty)constructor.Invoke(new object?[] { name, majorId });
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            // Propagate the actual domain exception thrown by validation rules back out to the test framework assertions
            throw ex.InnerException;
        }
    }

    #endregion
}
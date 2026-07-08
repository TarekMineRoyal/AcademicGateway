using System;
using AcademicGateway.Domain.Students;
using AcademicGateway.Domain.Students.Exceptions;
using FluentAssertions;
using Xunit;

namespace AcademicGateway.Domain.UnitTests.Students;

public class StudentTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeCorrectly_WhenParametersAreValid()
    {
        // Arrange
        var validId = Guid.NewGuid();
        var untrimmedName = "   John Doe   ";
        var expectedName = "John Doe";
        var validGraduationYear = 2026;

        // Act
        var student = new Student(validId, untrimmedName, validGraduationYear);

        // Assert
        student.Id.Should().Be(validId);
        student.FullName.Should().Be(expectedName);
        student.GraduationYear.Should().Be(validGraduationYear);
        student.StudentMajors.Should().BeEmpty();
        student.StudentSkills.Should().BeEmpty();
        student.StudentSpecialties.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidStudentDetailsException_WhenIdIsEmpty()
    {
        // Arrange
        var emptyId = Guid.Empty;

        // Act
        Action act = () => _ = new Student(emptyId, "Jane Doe");

        // Assert
        act.Should().Throw<InvalidStudentDetailsException>()
           .WithMessage("Identity User ID reference context cannot be empty.");
    }

    #endregion

    #region UpdateFullName Tests

    [Fact]
    public void UpdateFullName_ShouldModifyNameAndTrim_WhenNewNameIsValid()
    {
        // Arrange
        var student = new Student(Guid.NewGuid(), "Alice Smith");
        var untrimmedNewName = "  Alice M. Smith  ";
        var expectedName = "Alice M. Smith";

        // Act
        student.UpdateFullName(untrimmedNewName);

        // Assert
        student.FullName.Should().Be(expectedName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateFullName_ShouldThrowInvalidStudentDetailsException_WhenNewNameIsInvalid(string? invalidName)
    {
        // Arrange
        var student = new Student(Guid.NewGuid(), "Bob Jones");

        // Act
        Action act = () => student.UpdateFullName(invalidName!);

        // Assert
        act.Should().Throw<InvalidStudentDetailsException>()
           .WithMessage("Student identity name fields cannot be empty or whitespace.");
    }

    #endregion

    #region UpdateGraduationYear Tests

    [Theory]
    [InlineData(2000)]
    [InlineData(2030)]
    [InlineData(null)]
    public void UpdateGraduationYear_ShouldModifyYear_WhenYearIsValid(int? validYear)
    {
        // Arrange
        var student = new Student(Guid.NewGuid(), "Charlie Brown", 2025);

        // Act
        student.UpdateGraduationYear(validYear);

        // Assert
        student.GraduationYear.Should().Be(validYear);
    }

    [Fact]
    public void UpdateGraduationYear_ShouldThrowInvalidGraduationYearException_WhenYearIsBefore2000()
    {
        // Arrange
        var student = new Student(Guid.NewGuid(), "Charlie Brown", 2025);
        var invalidYear = 1999;

        // Act
        Action act = () => student.UpdateGraduationYear(invalidYear);

        // Assert
        act.Should().Throw<InvalidGraduationYearException>();
    }

    #endregion

    #region Major Management Tests

    [Fact]
    public void AddMajor_ShouldAttachLink_WhenIdIsValidAndUnique()
    {
        // Arrange
        var student = new Student(Guid.NewGuid(), "Diana Prince");
        var majorId = Guid.NewGuid();

        // Act
        student.AddMajor(majorId);

        // Assert
        student.StudentMajors.Should().ContainSingle(sm => sm.MajorId == majorId);
    }

    [Fact]
    public void AddMajor_ShouldThrowInvalidStudentDetailsException_WhenIdIsEmpty()
    {
        // Arrange
        var student = new Student(Guid.NewGuid(), "Diana Prince");

        // Act
        Action act = () => student.AddMajor(Guid.Empty);

        // Assert
        act.Should().Throw<InvalidStudentDetailsException>()
           .WithMessage("Target reference major identification context cannot be empty.");
    }

    [Fact]
    public void AddMajor_ShouldNotAddDuplicate_WhenLinkAlreadyExists()
    {
        // Arrange
        var student = new Student(Guid.NewGuid(), "Diana Prince");
        var majorId = Guid.NewGuid();
        student.AddMajor(majorId);

        // Act
        student.AddMajor(majorId);

        // Assert
        student.StudentMajors.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveMajor_ShouldEvictLink_WhenLinkExists()
    {
        // Arrange
        var student = new Student(Guid.NewGuid(), "Diana Prince");
        var majorId = Guid.NewGuid();
        student.AddMajor(majorId);

        // Act
        student.RemoveMajor(majorId);

        // Assert
        student.StudentMajors.Should().BeEmpty();
    }

    [Fact]
    public void RemoveMajor_ShouldDoNothing_WhenLinkDoesNotExist()
    {
        // Arrange
        var student = new Student(Guid.NewGuid(), "Diana Prince");
        var activeMajorId = Guid.NewGuid();
        var nonExistentMajorId = Guid.NewGuid();
        student.AddMajor(activeMajorId);

        // Act
        student.RemoveMajor(nonExistentMajorId);

        // Assert
        student.StudentMajors.Should().HaveCount(1);
    }

    #endregion

    #region Skill Management Tests

    [Fact]
    public void AddSkill_ShouldAttachLink_WhenIdIsValidAndUnique()
    {
        // Arrange
        var student = new Student(Guid.NewGuid(), "Bruce Wayne");
        var skillId = Guid.NewGuid();

        // Act
        student.AddSkill(skillId);

        // Assert
        student.StudentSkills.Should().ContainSingle(ss => ss.SkillId == skillId);
    }

    [Fact]
    public void AddSkill_ShouldThrowInvalidStudentDetailsException_WhenIdIsEmpty()
    {
        // Arrange
        var student = new Student(Guid.NewGuid(), "Bruce Wayne");

        // Act
        Action act = () => student.AddSkill(Guid.Empty);

        // Assert
        act.Should().Throw<InvalidStudentDetailsException>()
           .WithMessage("Target reference skill identification context cannot be empty.");
    }

    [Fact]
    public void AddSkill_ShouldNotAddDuplicate_WhenLinkAlreadyExists()
    {
        // Arrange
        var student = new Student(Guid.NewGuid(), "Bruce Wayne");
        var skillId = Guid.NewGuid();
        student.AddSkill(skillId);

        // Act
        student.AddSkill(skillId);

        // Assert
        student.StudentSkills.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveSkill_ShouldEvictLink_WhenLinkExists()
    {
        // Arrange
        var student = new Student(Guid.NewGuid(), "Bruce Wayne");
        var skillId = Guid.NewGuid();
        student.AddSkill(skillId);

        // Act
        student.RemoveSkill(skillId);

        // Assert
        student.StudentSkills.Should().BeEmpty();
    }

    [Fact]
    public void RemoveSkill_ShouldDoNothing_WhenLinkDoesNotExist()
    {
        // Arrange
        var student = new Student(Guid.NewGuid(), "Bruce Wayne");
        var activeSkillId = Guid.NewGuid();
        var nonExistentSkillId = Guid.NewGuid();
        student.AddSkill(activeSkillId);

        // Act
        student.RemoveSkill(nonExistentSkillId);

        // Assert
        student.StudentSkills.Should().HaveCount(1);
    }

    #endregion

    #region Specialty Management Tests

    [Fact]
    public void AddSpecialty_ShouldAttachLink_WhenIdIsValidAndUnique()
    {
        // Arrange
        var student = new Student(Guid.NewGuid(), "Clark Kent");
        var specialtyId = Guid.NewGuid();

        // Act
        student.AddSpecialty(specialtyId);

        // Assert
        student.StudentSpecialties.Should().ContainSingle(ss => ss.SpecialtyId == specialtyId);
    }

    [Fact]
    public void AddSpecialty_ShouldThrowInvalidStudentDetailsException_WhenIdIsEmpty()
    {
        // Arrange
        var student = new Student(Guid.NewGuid(), "Clark Kent");

        // Act
        Action act = () => student.AddSpecialty(Guid.Empty);

        // Assert
        act.Should().Throw<InvalidStudentDetailsException>()
           .WithMessage("Target reference sub-track specialty identification context cannot be empty.");
    }

    [Fact]
    public void AddSpecialty_ShouldNotAddDuplicate_WhenLinkAlreadyExists()
    {
        // Arrange
        var student = new Student(Guid.NewGuid(), "Clark Kent");
        var specialtyId = Guid.NewGuid();
        student.AddSpecialty(specialtyId);

        // Act
        student.AddSpecialty(specialtyId);

        // Assert
        student.StudentSpecialties.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveSpecialty_ShouldEvictLink_WhenLinkExists()
    {
        // Arrange
        var student = new Student(Guid.NewGuid(), "Clark Kent");
        var specialtyId = Guid.NewGuid();
        student.AddSpecialty(specialtyId);

        // Act
        student.RemoveSpecialty(specialtyId);

        // Assert
        student.StudentSpecialties.Should().BeEmpty();
    }

    [Fact]
    public void RemoveSpecialty_ShouldDoNothing_WhenLinkDoesNotExist()
    {
        // Arrange
        var student = new Student(Guid.NewGuid(), "Clark Kent");
        var activeSpecialtyId = Guid.NewGuid();
        var nonExistentSpecialtyId = Guid.NewGuid();
        student.AddSpecialty(activeSpecialtyId);

        // Act
        student.RemoveSpecialty(nonExistentSpecialtyId);

        // Assert
        student.StudentSpecialties.Should().HaveCount(1);
    }

    #endregion
}
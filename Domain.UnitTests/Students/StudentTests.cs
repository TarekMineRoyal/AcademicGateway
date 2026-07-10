using System;
using System.Linq;
using System.Reflection;
using Xunit;
using FluentAssertions;
using AcademicGateway.Domain.Students;
using AcademicGateway.Domain.Students.Exceptions;

namespace AcademicGateway.Domain.Tests.Students;

public class StudentTests
{
    private readonly Guid _validStudentId = Guid.NewGuid();
    private readonly string _validFullName = "John Constantine Doe";
    private readonly int _validGraduationYear = 2028;

    #region Constructor & Initialization Tests

    [Fact]
    public void Constructor_WithValidParameters_ShouldInitializeStudentCorrectly()
    {
        // Arrange & Act
        var student = new Student(_validStudentId, _validFullName, _validGraduationYear);

        // Assert
        student.Id.Should().Be(_validStudentId);
        student.FullName.Should().Be(_validFullName);
        student.GraduationYear.Should().Be(_validGraduationYear);
        student.StudentMajors.Should().BeEmpty();
        student.StudentSkills.Should().BeEmpty();
        student.StudentSpecialties.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNullGraduationYear_ShouldInitializeSuccessfully()
    {
        // Arrange & Act
        var student = new Student(_validStudentId, _validFullName, graduationYear: null);

        // Assert
        student.GraduationYear.Should().BeNull();
    }

    [Fact]
    public void Constructor_WhenIdIsEmptyGuid_ShouldThrowInvalidStudentDetailsException()
    {
        // Arrange
        var emptyId = Guid.Empty;

        // Act
        Action act = () => new Student(emptyId, _validFullName, _validGraduationYear);

        // Assert
        act.Should().Throw<InvalidStudentDetailsException>()
           .WithMessage("Identity User ID reference context cannot be empty.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WhenFullNameIsInvalid_ShouldThrowInvalidStudentDetailsException(string? invalidName)
    {
        // Act
        Action act = () => new Student(_validStudentId, invalidName!, _validGraduationYear);

        // Assert
        act.Should().Throw<InvalidStudentDetailsException>()
           .WithMessage("Student identity name fields cannot be empty or whitespace.");
    }

    [Fact]
    public void Constructor_WhenFullNameHasWhitespaces_ShouldTrimInputString()
    {
        // Arrange
        var untrimmedName = "   Jane Francesca Smith   ";

        // Act
        var student = new Student(_validStudentId, untrimmedName, _validGraduationYear);

        // Assert
        student.FullName.Should().Be("Jane Francesca Smith");
    }

    [Fact]
    public void Constructor_WhenGraduationYearIsExactlyAtBoundary2000_ShouldInitializeSuccessfully()
    {
        // Arrange
        var boundaryYear = 2000;

        // Act
        var student = new Student(_validStudentId, _validFullName, boundaryYear);

        // Assert
        student.GraduationYear.Should().Be(boundaryYear);
    }

    [Fact]
    public void Constructor_WhenGraduationYearIsBelowBoundary2000_ShouldThrowInvalidGraduationYearException()
    {
        // Arrange
        var invalidYear = 1999;

        // Act
        Action act = () => new Student(_validStudentId, _validFullName, invalidYear);

        // Assert
        act.Should().Throw<InvalidGraduationYearException>()
           .WithMessage($"The expected graduation year '{invalidYear}' is invalid. Dates cannot precede the year 2000 threshold.");
    }

    #endregion

    #region UpdateFullName Tests

    [Fact]
    public void UpdateFullName_WithValidName_ShouldUpdateAndTrimProperties()
    {
        // Arrange
        var student = new Student(_validStudentId, _validFullName, _validGraduationYear);
        var newName = "  Dr. Fate Logan  ";

        // Act
        student.UpdateFullName(newName);

        // Assert
        student.FullName.Should().Be("Dr. Fate Logan");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateFullName_WithNullEmptyOrWhitespace_ShouldThrowInvalidStudentDetailsException(string? invalidName)
    {
        // Arrange
        var student = new Student(_validStudentId, _validFullName, _validGraduationYear);

        // Act
        Action act = () => student.UpdateFullName(invalidName!);

        // Assert
        act.Should().Throw<InvalidStudentDetailsException>()
           .WithMessage("Student identity name fields cannot be empty or whitespace.");
    }

    #endregion

    #region UpdateGraduationYear Tests

    [Fact]
    public void UpdateGraduationYear_WithValidYear_ShouldUpdateSuccessfully()
    {
        // Arrange
        var student = new Student(_validStudentId, _validFullName, _validGraduationYear);
        var targetYear = 2035;

        // Act
        student.UpdateGraduationYear(targetYear);

        // Assert
        student.GraduationYear.Should().Be(targetYear);
    }

    [Fact]
    public void UpdateGraduationYear_WithNullValue_ShouldResetPropertyToNull()
    {
        // Arrange
        var student = new Student(_validStudentId, _validFullName, _validGraduationYear);

        // Act
        student.UpdateGraduationYear(null);

        // Assert
        student.GraduationYear.Should().BeNull();
    }

    [Fact]
    public void UpdateGraduationYear_WhenValueIsBelowBoundary2000_ShouldThrowInvalidGraduationYearException()
    {
        // Arrange
        var student = new Student(_validStudentId, _validFullName, _validGraduationYear);
        var invalidYear = 1999;

        // Act
        Action act = () => student.UpdateGraduationYear(invalidYear);

        // Assert
        act.Should().Throw<InvalidGraduationYearException>()
           .WithMessage($"The expected graduation year '{invalidYear}' is invalid. Dates cannot precede the year 2000 threshold.");
    }

    #endregion

    #region Academic Major Management Tests

    [Fact]
    public void AddMajor_WithValidId_ShouldAppendToStudentMajorsCollection()
    {
        // Arrange
        var student = new Student(_validStudentId, _validFullName, _validGraduationYear);
        var majorId = Guid.NewGuid();

        // Act
        student.AddMajor(majorId);

        // Assert
        student.StudentMajors.Should().HaveCount(1);
        var mapping = student.StudentMajors.First();
        mapping.StudentId.Should().Be(student.Id);
        mapping.MajorId.Should().Be(majorId);
    }

    [Fact]
    public void AddMajor_WhenMajorIdIsEmptyGuid_ShouldThrowInvalidStudentDetailsException()
    {
        // Arrange
        var student = new Student(_validStudentId, _validFullName, _validGraduationYear);

        // Act
        Action act = () => student.AddMajor(Guid.Empty);

        // Assert
        act.Should().Throw<InvalidStudentDetailsException>()
           .WithMessage("Target reference major identification context cannot be empty.");
    }

    [Fact]
    public void AddMajor_WhenMajorIdAlreadyExists_ShouldReturnSilentlyWithoutDuplicating()
    {
        // Arrange
        var student = new Student(_validStudentId, _validFullName, _validGraduationYear);
        var majorId = Guid.NewGuid();
        student.AddMajor(majorId);

        // Act
        student.AddMajor(majorId);

        // Assert
        student.StudentMajors.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveMajor_WhenMappingExists_ShouldSuccessfullyRemoveFromCollection()
    {
        // Arrange
        var student = new Student(_validStudentId, _validFullName, _validGraduationYear);
        var majorId = Guid.NewGuid();
        student.AddMajor(majorId);

        // Act
        student.RemoveMajor(majorId);

        // Assert
        student.StudentMajors.Should().BeEmpty();
    }

    [Fact]
    public void RemoveMajor_WhenMappingDoesNotExist_ShouldReturnSilentlyNoOp()
    {
        // Arrange
        var student = new Student(_validStudentId, _validFullName, _validGraduationYear);
        var majorIdInCollection = Guid.NewGuid();
        var nonExistentMajorId = Guid.NewGuid();
        student.AddMajor(majorIdInCollection);

        // Act
        student.RemoveMajor(nonExistentMajorId);

        // Assert
        student.StudentMajors.Should().HaveCount(1);
        student.StudentMajors.First().MajorId.Should().Be(majorIdInCollection);
    }

    #endregion

    #region Technical Skill Management Tests

    [Fact]
    public void AddSkill_WithValidId_ShouldAppendToStudentSkillsCollection()
    {
        // Arrange
        var student = new Student(_validStudentId, _validFullName, _validGraduationYear);
        var skillId = Guid.NewGuid();

        // Act
        student.AddSkill(skillId);

        // Assert
        student.StudentSkills.Should().HaveCount(1);
        var mapping = student.StudentSkills.First();
        mapping.StudentId.Should().Be(student.Id);
        mapping.SkillId.Should().Be(skillId);
    }

    [Fact]
    public void AddSkill_WhenSkillIdIsEmptyGuid_ShouldThrowInvalidStudentDetailsException()
    {
        // Arrange
        var student = new Student(_validStudentId, _validFullName, _validGraduationYear);

        // Act
        Action act = () => student.AddSkill(Guid.Empty);

        // Assert
        act.Should().Throw<InvalidStudentDetailsException>()
           .WithMessage("Target reference skill identification context cannot be empty.");
    }

    [Fact]
    public void AddSkill_WhenSkillIdAlreadyExists_ShouldReturnSilentlyWithoutDuplicating()
    {
        // Arrange
        var student = new Student(_validStudentId, _validFullName, _validGraduationYear);
        var skillId = Guid.NewGuid();
        student.AddSkill(skillId);

        // Act
        student.AddSkill(skillId);

        // Assert
        student.StudentSkills.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveSkill_WhenMappingExists_ShouldSuccessfullyRemoveFromCollection()
    {
        // Arrange
        var student = new Student(_validStudentId, _validFullName, _validGraduationYear);
        var skillId = Guid.NewGuid();
        student.AddSkill(skillId);

        // Act
        student.RemoveSkill(skillId);

        // Assert
        student.StudentSkills.Should().BeEmpty();
    }

    [Fact]
    public void RemoveSkill_WhenMappingDoesNotExist_ShouldReturnSilentlyNoOp()
    {
        // Arrange
        var student = new Student(_validStudentId, _validFullName, _validGraduationYear);
        var skillIdInCollection = Guid.NewGuid();
        var nonExistentSkillId = Guid.NewGuid();
        student.AddSkill(skillIdInCollection);

        // Act
        student.RemoveSkill(nonExistentSkillId);

        // Assert
        student.StudentSkills.Should().HaveCount(1);
        student.StudentSkills.First().SkillId.Should().Be(skillIdInCollection);
    }

    #endregion

    #region Academic Specialty Management Tests

    [Fact]
    public void AddSpecialty_WithValidId_ShouldAppendToStudentSpecialtiesCollection()
    {
        // Arrange
        var student = new Student(_validStudentId, _validFullName, _validGraduationYear);
        var specialtyId = Guid.NewGuid();

        // Act
        student.AddSpecialty(specialtyId);

        // Assert
        student.StudentSpecialties.Should().HaveCount(1);
        var mapping = student.StudentSpecialties.First();
        mapping.StudentId.Should().Be(student.Id);
        mapping.SpecialtyId.Should().Be(specialtyId);
    }

    [Fact]
    public void AddSpecialty_WhenSpecialtyIdIsEmptyGuid_ShouldThrowInvalidStudentDetailsException()
    {
        // Arrange
        var student = new Student(_validStudentId, _validFullName, _validGraduationYear);

        // Act
        Action act = () => student.AddSpecialty(Guid.Empty);

        // Assert
        act.Should().Throw<InvalidStudentDetailsException>()
           .WithMessage("Target reference sub-track specialty identification context cannot be empty.");
    }

    [Fact]
    public void AddSpecialty_WhenSpecialtyIdAlreadyExists_ShouldReturnSilentlyWithoutDuplicating()
    {
        // Arrange
        var student = new Student(_validStudentId, _validFullName, _validGraduationYear);
        var specialtyId = Guid.NewGuid();
        student.AddSpecialty(specialtyId);

        // Act
        student.AddSpecialty(specialtyId);

        // Assert
        student.StudentSpecialties.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveSpecialty_WhenMappingExists_ShouldSuccessfullyRemoveFromCollection()
    {
        // Arrange
        var student = new Student(_validStudentId, _validFullName, _validGraduationYear);
        var specialtyId = Guid.NewGuid();
        student.AddSpecialty(specialtyId);

        // Act
        student.RemoveSpecialty(specialtyId);

        // Assert
        student.StudentSpecialties.Should().BeEmpty();
    }

    [Fact]
    public void RemoveSpecialty_WhenMappingDoesNotExist_ShouldReturnSilentlyNoOp()
    {
        // Arrange
        var student = new Student(_validStudentId, _validFullName, _validGraduationYear);
        var specialtyIdInCollection = Guid.NewGuid();
        var nonExistentSpecialtyId = Guid.NewGuid();
        student.AddSpecialty(specialtyIdInCollection);

        // Act
        student.RemoveSpecialty(nonExistentSpecialtyId);

        // Assert
        student.StudentSpecialties.Should().HaveCount(1);
        student.StudentSpecialties.First().SpecialtyId.Should().Be(specialtyIdInCollection);
    }

    #endregion

    #region Entity Framework Hydration & Aggregate Integrity Tests

    [Fact]
    public void EFCoreHydration_ViaParameterlessConstructor_ExposesCorruptedInvariants()
    {
        // Arrange & Act
        var rehydratedStudent = (Student)Activator.CreateInstance(typeof(Student), nonPublic: true)!;

        // Assert Invariant Corruptions
        rehydratedStudent.Id.Should().Be(Guid.Empty);
        rehydratedStudent.FullName.Should().Be(string.Empty);
        rehydratedStudent.GraduationYear.Should().BeNull();
        rehydratedStudent.StudentMajors.Should().NotBeNull();
        rehydratedStudent.StudentSkills.Should().NotBeNull();
        rehydratedStudent.StudentSpecialties.Should().NotBeNull();
    }

    [Fact]
    public void EFCoreHydration_WhenAddingCollectionsOnUnpopulatedInstance_ThrowsCascadingException()
    {
        // Arrange
        var rehydratedStudent = (Student)Activator.CreateInstance(typeof(Student), nonPublic: true)!;
        var validMajorId = Guid.NewGuid();

        // Act & Assert
        Action act = () => rehydratedStudent.AddMajor(validMajorId);
        act.Should().Throw<InvalidStudentDetailsException>()
           .WithMessage("Student tracking identity parameters cannot be empty mapping references.");
    }

    #endregion
}
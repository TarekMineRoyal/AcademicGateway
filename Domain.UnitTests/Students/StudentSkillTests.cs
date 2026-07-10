using System;
using Xunit;
using FluentAssertions;
using AcademicGateway.Domain.Students;
using AcademicGateway.Domain.Students.Exceptions;

namespace AcademicGateway.Domain.Tests.Students;

public class StudentSkillTests
{
    [Fact]
    public void Constructor_WithValidIdentifiers_ShouldInitializeSuccessfully()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var skillId = Guid.NewGuid();

        // Act
        var mapping = new StudentSkill(studentId, skillId);

        // Assert
        mapping.StudentId.Should().Be(studentId);
        mapping.SkillId.Should().Be(skillId);
        mapping.Student.Should().BeNull();
        mapping.Skill.Should().BeNull();
    }

    [Fact]
    public void Constructor_WhenStudentIdIsEmpty_ShouldThrowInvalidStudentDetailsException()
    {
        // Arrange
        var skillId = Guid.NewGuid();

        // Act
        Action act = () => new StudentSkill(Guid.Empty, skillId);

        // Assert
        act.Should().Throw<InvalidStudentDetailsException>()
           .WithMessage("Student tracking identity parameters cannot be empty mapping references.");
    }

    [Fact]
    public void Constructor_WhenSkillIdIsEmpty_ShouldThrowInvalidStudentDetailsException()
    {
        // Arrange
        var studentId = Guid.NewGuid();

        // Act
        Action act = () => new StudentSkill(studentId, Guid.Empty);

        // Assert
        act.Should().Throw<InvalidStudentDetailsException>()
           .WithMessage("Competency skill tracking identity parameters cannot be empty mapping references.");
    }

    [Fact]
    public void EFCoreHydration_ViaParameterlessConstructor_LeavesPropertiesUninitialized()
    {
        // Arrange & Act
        var materialized = (StudentSkill)Activator.CreateInstance(typeof(StudentSkill), nonPublic: true)!;

        // Assert
        materialized.StudentId.Should().Be(Guid.Empty);
        materialized.SkillId.Should().Be(Guid.Empty);
    }
}
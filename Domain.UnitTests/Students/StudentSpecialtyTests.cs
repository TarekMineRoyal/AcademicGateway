using System;
using Xunit;
using FluentAssertions;
using AcademicGateway.Domain.Students;
using AcademicGateway.Domain.Students.Exceptions;

namespace AcademicGateway.Domain.Tests.Students;

public class StudentSpecialtyTests
{
    [Fact]
    public void Constructor_WithValidIdentifiers_ShouldInitializeSuccessfully()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var specialtyId = Guid.NewGuid();

        // Act
        var mapping = new StudentSpecialty(studentId, specialtyId);

        // Assert
        mapping.StudentId.Should().Be(studentId);
        mapping.SpecialtyId.Should().Be(specialtyId);
        mapping.Student.Should().BeNull();
        mapping.Specialty.Should().BeNull();
    }

    [Fact]
    public void Constructor_WhenStudentIdIsEmpty_ShouldThrowInvalidStudentDetailsException()
    {
        // Arrange
        var specialtyId = Guid.NewGuid();

        // Act
        Action act = () => new StudentSpecialty(Guid.Empty, specialtyId);

        // Assert
        act.Should().Throw<InvalidStudentDetailsException>()
           .WithMessage("Student tracking identity parameters cannot be empty mapping references.");
    }

    [Fact]
    public void Constructor_WhenSpecialtyIdIsEmpty_ShouldThrowInvalidStudentDetailsException()
    {
        // Arrange
        var studentId = Guid.NewGuid();

        // Act
        Action act = () => new StudentSpecialty(studentId, Guid.Empty);

        // Assert
        act.Should().Throw<InvalidStudentDetailsException>()
           .WithMessage("Sub-track specialty tracking identity parameters cannot be empty mapping references.");
    }

    [Fact]
    public void EFCoreHydration_ViaParameterlessConstructor_LeavesPropertiesUninitialized()
    {
        // Arrange & Act
        var materialized = (StudentSpecialty)Activator.CreateInstance(typeof(StudentSpecialty), nonPublic: true)!;

        // Assert
        materialized.StudentId.Should().Be(Guid.Empty);
        materialized.SpecialtyId.Should().Be(Guid.Empty);
    }
}
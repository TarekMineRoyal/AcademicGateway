using System;
using Xunit;
using FluentAssertions;
using AcademicGateway.Domain.Students;
using AcademicGateway.Domain.Students.Exceptions;

namespace AcademicGateway.Domain.Tests.Students;

public class StudentMajorTests
{
    [Fact]
    public void Constructor_WithValidIdentifiers_ShouldInitializeSuccessfully()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var majorId = Guid.NewGuid();

        // Act
        var mapping = new StudentMajor(studentId, majorId);

        // Assert
        mapping.StudentId.Should().Be(studentId);
        mapping.MajorId.Should().Be(majorId);
        mapping.Student.Should().BeNull();
        mapping.Major.Should().BeNull();
    }

    [Fact]
    public void Constructor_WhenStudentIdIsEmpty_ShouldThrowInvalidStudentDetailsException()
    {
        // Arrange
        var majorId = Guid.NewGuid();

        // Act
        Action act = () => new StudentMajor(Guid.Empty, majorId);

        // Assert
        act.Should().Throw<InvalidStudentDetailsException>()
           .WithMessage("Student tracking identity parameters cannot be empty mapping references.");
    }

    [Fact]
    public void Constructor_WhenMajorIdIsEmpty_ShouldThrowInvalidStudentDetailsException()
    {
        // Arrange
        var studentId = Guid.NewGuid();

        // Act
        Action act = () => new StudentMajor(studentId, Guid.Empty);

        // Assert
        act.Should().Throw<InvalidStudentDetailsException>()
           .WithMessage("Academic major tracking identity parameters cannot be empty mapping references.");
    }

    [Fact]
    public void EFCoreHydration_ViaParameterlessConstructor_LeavesPropertiesUninitialized()
    {
        // Arrange & Act
        var materialized = (StudentMajor)Activator.CreateInstance(typeof(StudentMajor), nonPublic: true)!;

        // Assert
        materialized.StudentId.Should().Be(Guid.Empty);
        materialized.MajorId.Should().Be(Guid.Empty);
    }
}
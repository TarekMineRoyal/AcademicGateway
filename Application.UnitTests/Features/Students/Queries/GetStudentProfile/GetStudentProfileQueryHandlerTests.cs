using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.Students.Queries.GetStudentProfile;
using AcademicGateway.Domain.Curriculum;
using AcademicGateway.Domain.Skills;
using AcademicGateway.Domain.Students;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Students.Queries.GetStudentProfile;

/// <summary>
/// Contains isolated unit verification routines for the <see cref="GetStudentProfileQueryHandler"/>.
/// Validates deep relational untracked database select projections, DTO value mapping, 
/// missing context exceptions, and projection ternary null fallback descriptors.
/// </summary>
public class GetStudentProfileQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly GetStudentProfileQueryHandler _handler;

    /// <summary>
    /// Initializes a pristine instance of the test suite with isolated database mock contexts.
    /// </summary>
    public GetStudentProfileQueryHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _handler = new GetStudentProfileQueryHandler(_dbContextMock.Object);
    }

    /// <summary>
    /// Assures that querying for an existing student profile accurately executes relational LINQ join 
    /// projections to yield a comprehensive <see cref="StudentProfileDto"/> mapping majors, specialties, and skills.
    /// </summary>
    [Fact]
    public async Task Handle_GivenExistingStudentId_ShouldReturnDeepProjectedProfileDto()
    {
        // Arrange
        var targetStudentId = Guid.NewGuid();
        var majorId = Guid.NewGuid();
        var specialtyId = Guid.NewGuid();
        var skillId = Guid.NewGuid();

        var student = new Student(
            id: targetStudentId,
            fullName: "Jane Doe",
            graduationYear: 2026
        );

        student.AddMajor(majorId);
        student.AddSpecialty(specialtyId);
        student.AddSkill(skillId);

        // Simulate database lookup navigation targets for the projection compiler by assigning nested entity names via reflection
        var mockMajor = CreateEntityWithPrivateConstructor<Major>();
        SetPrivateProperty(mockMajor, nameof(Major.Name), "Computer Science");
        SetPrivateProperty(student.StudentMajors.First(), nameof(StudentMajor.Major), mockMajor);

        var mockSpecialty = CreateEntityWithPrivateConstructor<Specialty>();
        SetPrivateProperty(mockSpecialty, nameof(Specialty.Name), "Artificial Intelligence");
        SetPrivateProperty(student.StudentSpecialties.First(), nameof(StudentSpecialty.Specialty), mockSpecialty);

        var mockSkill = CreateEntityWithPrivateConstructor<Skill>();
        SetPrivateProperty(mockSkill, nameof(Skill.Name), "C#");
        SetPrivateProperty(student.StudentSkills.First(), nameof(StudentSkill.Skill), mockSkill);

        var studentsList = new List<Student> { student };
        var mockDbSet = studentsList.BuildMockDbSet();
        _dbContextMock.Setup(db => db.Students).Returns(mockDbSet.Object);

        var query = new GetStudentProfileQuery(targetStudentId);

        // Act
        var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(targetStudentId);
        result.FullName.Should().Be("Jane Doe");
        result.GraduationYear.Should().Be(2026);

        result.Majors.Should().ContainSingle(m => m.Id == majorId && m.Name == "Computer Science");
        result.Specialties.Should().ContainSingle(s => s.Id == specialtyId && s.Name == "Artificial Intelligence");
        result.Skills.Should().ContainSingle(s => s.Id == skillId && s.Name == "C#");
    }

    /// <summary>
    /// Assures that executing a lookup query for a missing or non-existent student primary identity code
    /// cleanly interrupts the request flow and throws a targeted <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNonExistentStudentId_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var mockDbSet = new List<Student>().BuildMockDbSet();
        _dbContextMock.Setup(db => db.Students).Returns(mockDbSet.Object);

        var wrongId = Guid.NewGuid();
        var query = new GetStudentProfileQuery(wrongId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*Student profile for ID '{wrongId}' was not found within the institutional directory.*");
    }

    /// <summary>
    /// Assures that if a student's major relational join context row does not hold a hydrated parent Major entity model,
    /// the dynamic projection safely applies the custom string ternary fallback option "Unknown Major".
    /// </summary>
    [Fact]
    public async Task Handle_GivenStudentWithNullMajorNavigationProperty_ShouldMapNameAsUnknownMajor()
    {
        // Arrange
        var targetStudentId = Guid.NewGuid();
        var majorId = Guid.NewGuid();

        var student = new Student(targetStudentId, "Jane Doe", 2026);
        student.AddMajor(majorId);

        // Explicit Boundary Condition: Force the parent 'Major' navigation entity to remain null
        SetPrivateProperty(student.StudentMajors.First(), nameof(StudentMajor.Major), null!);

        var mockDbSet = new List<Student> { student }.BuildMockDbSet();
        _dbContextMock.Setup(db => db.Students).Returns(mockDbSet.Object);

        var query = new GetStudentProfileQuery(targetStudentId);

        // Act
        var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Majors.Should().ContainSingle(m => m.Id == majorId && m.Name == "Unknown Major");
    }

    /// <summary>
    /// Assures that if a student's specialty relational join context row does not hold a hydrated Specialty entity model,
    /// the dynamic projection safely applies the custom string ternary fallback option "Unknown Specialty".
    /// </summary>
    [Fact]
    public async Task Handle_GivenStudentWithNullSpecialtyNavigationProperty_ShouldMapNameAsUnknownSpecialty()
    {
        // Arrange
        var targetStudentId = Guid.NewGuid();
        var specialtyId = Guid.NewGuid();

        var student = new Student(targetStudentId, "Jane Doe", 2026);
        student.AddSpecialty(specialtyId);

        // Explicit Boundary Condition: Force the child 'Specialty' navigation entity to remain null
        SetPrivateProperty(student.StudentSpecialties.First(), nameof(StudentSpecialty.Specialty), null!);

        var mockDbSet = new List<Student> { student }.BuildMockDbSet();
        _dbContextMock.Setup(db => db.Students).Returns(mockDbSet.Object);

        var query = new GetStudentProfileQuery(targetStudentId);

        // Act
        var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Specialties.Should().ContainSingle(s => s.Id == specialtyId && s.Name == "Unknown Specialty");
    }

    /// <summary>
    /// Assures that if a student's skill relational join context row does not hold a hydrated Skill entity model,
    /// the dynamic projection safely applies the custom string ternary fallback option "Unknown Skill".
    /// </summary>
    [Fact]
    public async Task Handle_GivenStudentWithNullSkillNavigationProperty_ShouldMapNameAsUnknownSkill()
    {
        // Arrange
        var targetStudentId = Guid.NewGuid();
        var skillId = Guid.NewGuid();

        var student = new Student(targetStudentId, "Jane Doe", 2026);
        student.AddSkill(skillId);

        // Explicit Boundary Condition: Force the lookup 'Skill' navigation entity to remain null
        SetPrivateProperty(student.StudentSkills.First(), nameof(StudentSkill.Skill), null!);

        var mockDbSet = new List<Student> { student }.BuildMockDbSet();
        _dbContextMock.Setup(db => db.Students).Returns(mockDbSet.Object);

        var query = new GetStudentProfileQuery(targetStudentId);

        // Act
        var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Skills.Should().ContainSingle(s => s.Id == skillId && s.Name == "Unknown Skill");
    }

    private static T CreateEntityWithPrivateConstructor<T>() =>
        (T)Activator.CreateInstance(typeof(T), true)!;

    private static void SetPrivateProperty(object obj, string propertyName, object? value) =>
        obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?
           .SetValue(obj, value);
}
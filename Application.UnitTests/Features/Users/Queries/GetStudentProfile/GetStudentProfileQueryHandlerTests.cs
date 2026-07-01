using AcademicGateway.Application.Common.Interfaces;
using Application.Features.Students.Queries.GetStudentProfile;
using Domain.Models.Academic;
using Domain.Students;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Users.Queries.GetStudentProfile;

public class GetStudentProfileQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly GetStudentProfileQueryHandler _handler;

    public GetStudentProfileQueryHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _handler = new GetStudentProfileQueryHandler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Profile_When_Student_Exists()
    {
        // Arrange
        var targetUserId = "student-123";
        var majorId = Guid.NewGuid();
        var specialtyId = Guid.NewGuid();
        var skillId = Guid.NewGuid();

        var students = new List<Student>
        {
            new Student
            {
                UserId = targetUserId,
                GraduationYear = 2026,
                StudentMajors = new List<StudentMajor>
                {
                    new StudentMajor { MajorId = majorId, Major = new Major { Id = majorId, Name = "Computer Science" } }
                },
                StudentSpecialties = new List<StudentSpecialty>
                {
                    new StudentSpecialty { SpecialtyId = specialtyId, Specialty = new Specialty { Id = specialtyId, Name = "Artificial Intelligence" } }
                },
                StudentSkills = new List<StudentSkill>
                {
                    new StudentSkill { SkillId = skillId, Skill = new Skill { Id = skillId, Name = "C#" } }
                }
            }
        };

        var mockDbSet = students.BuildMockDbSet();
        _dbContextMock.Setup(db => db.Students).Returns(mockDbSet.Object);

        var query = new GetStudentProfileQuery(targetUserId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(targetUserId);
        result.GraduationYear.Should().Be(2026);

        result.Majors.Should().ContainSingle(m => m.Id == majorId && m.Name == "Computer Science");
        result.Specialties.Should().ContainSingle(s => s.Id == specialtyId && s.Name == "Artificial Intelligence");
        result.Skills.Should().ContainSingle(s => s.Id == skillId && s.Name == "C#");
    }

    [Fact]
    public async Task Handle_Should_Throw_KeyNotFoundException_When_Student_Does_Not_Exist()
    {
        // Arrange
        var mockDbSet = new List<Student>().BuildMockDbSet();
        _dbContextMock.Setup(db => db.Students).Returns(mockDbSet.Object);

        var query = new GetStudentProfileQuery("non-existent-id");

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*User ID 'non-existent-id' was not found.*");
    }
}
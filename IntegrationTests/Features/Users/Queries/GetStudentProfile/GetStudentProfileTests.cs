using AcademicGateway.Application.Features.Users.Queries.GetStudentProfile;
using AcademicGateway.Application.Features.Users.Commands.RegisterStudent;
using AcademicGateway.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace AcademicGateway.IntegrationTests.Features.Users.Queries.GetStudentProfile;

public class GetStudentProfileTests : BaseIntegrationTest
{
    public GetStudentProfileTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Should_ReturnStudentProfile_WhenUserIdExists()
    {
        // --- 1. ARRANGE ---
        var major = new Major { Id = Guid.NewGuid(), Name = "Computer Science" };
        var skill = new Skill { Id = Guid.NewGuid(), Name = "C#" };

        // Explicitly assign the MajorId to tie the specialty to our seeded major!
        var specialty = new Specialty
        {
            Id = Guid.NewGuid(),
            Name = "Software Engineering",
            MajorId = major.Id
        };

        await AddAsync(major);
        await AddAsync(specialty);
        await AddAsync(skill);

        var registerCommand = new RegisterStudentCommand
        {
            Email = "profile.student@academicgateway.com",
            Username = "profilestudent",
            Password = "SecurePassword123!",
            GraduationYear = 2026,
            MajorIds = new List<Guid> { major.Id }
        };

        var userId = await SendAsync(registerCommand);

        await AddAsync(new StudentSpecialty { StudentId = userId, SpecialtyId = specialty.Id });
        await AddAsync(new StudentSkill { StudentId = userId, SkillId = skill.Id });

        var query = new GetStudentProfileQuery(userId);

        // --- 2. ACT ---
        var result = await SendAsync(query);

        // --- 3. ASSERT ---
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.GraduationYear.Should().Be(2026);

        result.Majors.Should().ContainSingle(m => m.Id == major.Id && m.Name == "Computer Science");
        result.Specialties.Should().ContainSingle(s => s.Id == specialty.Id && s.Name == "Software Engineering");
        result.Skills.Should().ContainSingle(s => s.Id == skill.Id && s.Name == "C#");
    }
    
    [Fact]
    public async Task Should_ThrowKeyNotFoundException_WhenUserIdDoesNotExist()
    {
        // --- 1. ARRANGE ---
        var query = new GetStudentProfileQuery("non-existent-student-id");

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(query);

        // --- 3. ASSERT ---
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
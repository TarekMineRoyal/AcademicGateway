using AcademicGateway.Application.Features.Students.Commands.RegisterStudent;
using AcademicGateway.Application.Features.Students.Queries.GetStudentProfile;
using AcademicGateway.Domain.Curriculum;
using AcademicGateway.Domain.Skills;
using FluentAssertions;
using IntegrationTests.Infrastructure;
using Xunit;

namespace AcademicGateway.IntegrationTests.Subdomains.Students.Queries;

/// <summary>
/// Integration tests verifying lookups, relational data mapping accuracy, and error 
/// boundaries inside the GetStudentProfile query pipeline handler loop.
/// </summary>
[Collection("SharedDatabase")]
public class GetStudentProfileTests : BaseIntegrationTest
{
    public GetStudentProfileTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Ensures that querying for a student profile using an active, verified identifier returns 
    /// a structural DTO fully populated with linked Majors, Specialties, and Skill competencies.
    /// </summary>
    [Fact]
    public async Task Should_ReturnStudentProfile_WhenUserIdExists()
    {
        // --- 1. ARRANGE ---
        // Seed the curriculum infrastructure using clean DDD aggregate root patterns
        var major = new Major("Computer Science");
        major.AddSpecialty("Software Engineering");
        await AddAsync(major);

        // Safely extract the mapped child specialty instance to capture its tracking Guid
        var specialty = major.Specialties.Single(s => s.Name == "Software Engineering");

        // Seed an independent operational technical competency row
        var skill = new Skill("C#");
        await AddAsync(skill);

        // Build a complete registration command passing all relational identifiers inside the payload
        var registerCommand = new RegisterStudentCommand
        {
            Email = "profile.student@academicgateway.com",
            Username = "profilestudent",
            Password = "SecurePassword123!",
            GraduationYear = 2026,
            MajorIds = new List<Guid> { major.Id },
            SpecialtyIds = new List<Guid> { specialty.Id },
            SkillIds = new List<Guid> { skill.Id }
        };

        // Execution returns a strongly-typed Guid key identifier
        Guid studentId = await SendAsync(registerCommand);
        var query = new GetStudentProfileQuery(studentId);

        // --- 2. ACT ---
        var result = await SendAsync(query);

        // --- 3. ASSERT ---
        result.Should().NotBeNull();

        // Aligns with the standardized DTO 'Id' property contract naming conventions
        result.Id.Should().Be(studentId);
        result.GraduationYear.Should().Be(2026);

        // Verify nested read-model lookup collections map accurately
        result.Majors.Should().ContainSingle(m => m.Id == major.Id && m.Name == "Computer Science");
        result.Specialties.Should().ContainSingle(s => s.Id == specialty.Id && s.Name == "Software Engineering");
        result.Skills.Should().ContainSingle(s => s.Id == skill.Id && s.Name == "C#");
    }

    /// <summary>
    /// Ensures that dispatching a lookup query with a non-existent tracking reference 
    /// short-circuits gracefully at the data layer, throwing a <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Should_ThrowKeyNotFoundException_WhenUserIdDoesNotExist()
    {
        // --- 1. ARRANGE ---
        // Instantiated using a clean, unmapped random Guid value to challenge lookups
        var query = new GetStudentProfileQuery(Guid.NewGuid());

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(query);

        // --- 3. ASSERT ---
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
using AcademicGateway.Application.Features.Users.Commands.RegisterStudent;
using AcademicGateway.Domain.Entities;
using FluentAssertions;
using FluentValidation; // Needed for asserting ValidationException
using Xunit;

namespace AcademicGateway.IntegrationTests.Features.Users.Commands.RegisterStudent;

public class RegisterStudentTests : BaseIntegrationTest
{
    public RegisterStudentTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Should_RegisterStudentAndCreateProfile_WhenCommandIsValid()
    {
        // --- 1. ARRANGE ---
        // Seed lookups into the database first to satisfy PostgreSQL foreign key constraints
        var major = new Major { Id = Guid.NewGuid(), Name = "Computer Science" };
        var specialty = new Specialty { Id = Guid.NewGuid(), Name = "Software Engineering", MajorId = major.Id };
        var skill = new Skill { Id = Guid.NewGuid(), Name = "C# / .NET" };

        await AddAsync(major);
        await AddAsync(specialty);
        await AddAsync(skill);

        // Build a valid registration command referencing our seeded relational data
        var command = new RegisterStudentCommand
        {
            Email = "student.test@academicgateway.com",
            Username = "teststudent",
            Password = "SecurePassword123!",
            GraduationYear = 2026,
            MajorIds = new List<Guid> { major.Id },
            SpecialtyIds = new List<Guid> { specialty.Id },
            SkillIds = new List<Guid> { skill.Id }
        };

        // --- 2. ACT ---
        // Push the command straight through MediatR pipelines and behaviors
        string userId = await SendAsync(command);

        // --- 3. ASSERT ---
        // Verify a valid Identity User ID was returned from the handler
        userId.Should().NotBeNullOrEmpty();

        // Peek straight into the PostgreSQL database to check if the student row exists
        var studentProfile = await FindAsync<Student>(userId);

        studentProfile.Should().NotBeNull();
        studentProfile!.GraduationYear.Should().Be(command.GraduationYear);
        studentProfile.UserId.Should().Be(userId);

        // Note: If you want to check the relational join collections (StudentMajors, etc.),
        // you can query their lookup junction entities directly using FindAsync as well!
        var studentMajorRelation = await FindAsync<StudentMajor>(userId, major.Id);
        studentMajorRelation.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_ThrowValidationException_WhenRequiredFieldsAreMissing()
    {
        // --- 1. ARRANGE ---
        // Intentionally create a completely empty, invalid command
        var invalidCommand = new RegisterStudentCommand
        {
            Email = "",
            Username = "",
            Password = "",
            MajorIds = new List<Guid>() // Violates the .NotEmpty() rule
        };

        // --- 2. ACT & 3. ASSERT ---
        // Verify that the MediatR ValidationBehavior pipeline blocks it instantly
        Func<Task> act = async () => await SendAsync(invalidCommand);

        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.Any(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("required")))
            .Where(ex => ex.Errors.Any(e => e.PropertyName == "MajorIds" && e.ErrorMessage.Contains("at least one Major")));
    }

    [Fact]
    public async Task Should_ThrowValidationException_WhenSpecialtyDoesNotBelongToSelectedMajor()
    {
        // --- 1. ARRANGE ---
        // Create two separate majors
        var majorA = new Major { Id = Guid.NewGuid(), Name = "Computer Science" };
        var majorB = new Major { Id = Guid.NewGuid(), Name = "Mechanical Engineering" };

        // Tie the specialty strictly to Major B
        var specialtyForB = new Specialty { Id = Guid.NewGuid(), Name = "Thermodynamics", MajorId = majorB.Id };

        await AddAsync(majorA);
        await AddAsync(majorB);
        await AddAsync(specialtyForB);

        // User chooses Major A, but attempts to select a Specialty from Major B
        var maliciousCommand = new RegisterStudentCommand
        {
            Email = "crossref.test@academicgateway.com",
            Username = "crossreftest",
            Password = "SecurePassword123!",
            MajorIds = new List<Guid> { majorA.Id }, // Selected Major A
            SpecialtyIds = new List<Guid> { specialtyForB.Id } // Specialty belongs to Major B!
        };

        // --- 2. ACT & 3. ASSERT ---
        // Ensure the custom business rule catches the mismatch
        Func<Task> act = async () => await SendAsync(maliciousCommand);

        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.Any(e => e.PropertyName == "SpecialtyIds" && e.ErrorMessage.Contains("do not belong to your chosen majors")));
    }

    [Fact]
    public async Task Should_ThrowException_WhenEmailOrUsernameAlreadyExists()
    {
        // --- 1. ARRANGE ---
        var major = new Major { Id = Guid.NewGuid(), Name = "Business Information Systems" };
        await AddAsync(major);

        var command1 = new RegisterStudentCommand
        {
            Email = "duplicate.test@academicgateway.com",
            Username = "sharedusername",
            Password = "FirstPassword123!",
            MajorIds = new List<Guid> { major.Id }
        };

        var command2 = new RegisterStudentCommand
        {
            Email = "duplicate.test@academicgateway.com",
            Username = "sharedusername",
            Password = "SecondPassword123!",
            MajorIds = new List<Guid> { major.Id }
        };

        await SendAsync(command1);

        // --- 2. ACT & 3. ASSERT ---
        Func<Task> act = async () => await SendAsync(command2);

        // Reverted to your correct original exception text matcher
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*User creation failed*");
    }
}
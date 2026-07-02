using AcademicGateway.Application.Features.Students.Commands.RegisterStudent;
using AcademicGateway.Domain.Curriculum;
using AcademicGateway.Domain.Skills;
using AcademicGateway.Domain.Students;
using FluentAssertions;
using FluentValidation;
using IntegrationTests.Infrastructure;
using Xunit;

namespace AcademicGateway.IntegrationTests.Subdomains.Students.Commands;

/// <summary>
/// Integration tests verifying validation metrics, cross-aggregate relational constraints,
/// and profile provisioning side-effects inside the Student registration pipeline.
/// </summary>
[Collection("SharedDatabase")]
public class RegisterStudentTests : BaseIntegrationTest
{
    public RegisterStudentTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Ensures that executing a valid registration command correctly provisions both identity 
    /// tracking records and the linked, relational <see cref="Student"/> domain aggregate root.
    /// </summary>
    [Fact]
    public async Task Should_RegisterStudentAndCreateProfile_WhenCommandIsValid()
    {
        // --- 1. ARRANGE ---
        // Construct the major aggregate root and append child specialties via encapsulated methods
        var major = new Major("Computer Science");
        major.AddSpecialty("Software Engineering");
        await AddAsync(major);

        // Extract the internally tracked child specialty instance safely to capture its Guid identifier
        var specialty = major.Specialties.Single(s => s.Name == "Software Engineering");

        // Seed an independent technical capability lookup item
        var skill = new Skill("C# / .NET");
        await AddAsync(skill);

        // Build the command referencing our seeded relational lookups
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
        // Dispatch the payload straight through the application MediatR pipeline behaviors
        Guid studentId = await SendAsync(command);

        // --- 3. ASSERT ---
        studentId.Should().NotBeEmpty();

        // Direct query validation on the database to check student aggregate boundaries
        var studentProfile = await FindAsync<Student>(studentId);
        studentProfile.Should().NotBeNull();
        studentProfile!.GraduationYear.Should().Be(command.GraduationYear);

        // Verify relationship consistency using the aggregate's lookup collection bounds
        studentProfile.StudentMajors.Should().Contain(sm => sm.MajorId == major.Id);
    }

    /// <summary>
    /// Ensures that incoming payloads missing mandatory property inputs are rejected 
    /// early by validation pipeline behaviors, throwing a <see cref="ValidationException"/>.
    /// </summary>
    [Fact]
    public async Task Should_ThrowValidationException_WhenRequiredFieldsAreMissing()
    {
        // --- 1. ARRANGE ---
        var invalidCommand = new RegisterStudentCommand
        {
            Email = "",
            Username = "",
            Password = "",
            MajorIds = new List<Guid>() // Triggers mandatory non-empty item verification rules
        };

        // --- 2. ACT & 3. ASSERT ---
        Func<Task> act = async () => await SendAsync(invalidCommand);

        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.Any(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("required")))
            .Where(ex => ex.Errors.Any(e => e.PropertyName == "MajorIds" && e.ErrorMessage.Contains("at least one Major")));
    }

    /// <summary>
    /// Ensures that trying to map a specialty that does not belong to any of the 
    /// explicitly selected majors fails validation, throwing a <see cref="ValidationException"/>.
    /// </summary>
    [Fact]
    public async Task Should_ThrowValidationException_WhenSpecialtyDoesNotBelongToSelectedMajor()
    {
        // --- 1. ARRANGE ---
        // Seed two independent academic streams to check cross-reference safety boundaries
        var majorA = new Major("Computer Science");
        await AddAsync(majorA);

        var majorB = new Major("Mechanical Engineering");
        majorB.AddSpecialty("Thermodynamics"); // Encapsulated internally inside Major B boundary parameters
        await AddAsync(majorB);

        var specialtyForB = majorB.Specialties.Single(s => s.Name == "Thermodynamics");

        // Formulate an invalid command mapping Major A alongside an isolated sub-specialty from Major B
        var maliciousCommand = new RegisterStudentCommand
        {
            Email = "crossref.test@academicgateway.com",
            Username = "crossreftest",
            Password = "SecurePassword123!",
            MajorIds = new List<Guid> { majorA.Id }, // Chosen target program reference
            SpecialtyIds = new List<Guid> { specialtyForB.Id } // Violates constraint (belongs to Major B)
        };

        // --- 2. ACT & 3. ASSERT ---
        Func<Task> act = async () => await SendAsync(maliciousCommand);

        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.Any(e => e.PropertyName == "SpecialtyIds" && e.ErrorMessage.Contains("do not belong to your chosen majors")));
    }

    /// <summary>
    /// Ensures that trying to claim credentials that are already assigned to an existing profile 
    /// is caught and rejected at the identity layer, throwing an explicit security exception.
    /// </summary>
    [Fact]
    public async Task Should_ThrowException_WhenEmailOrUsernameAlreadyExists()
    {
        // --- 1. ARRANGE ---
        var major = new Major("Business Information Systems");
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

        // Seed the initial unique user entry into the storage cluster
        await SendAsync(command1);

        // --- 2. ACT & 3. ASSERT ---
        Func<Task> act = async () => await SendAsync(command2);

        // Catches security collisions and checks your exact exception text matcher signature rules
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*User creation failed*");
    }
}
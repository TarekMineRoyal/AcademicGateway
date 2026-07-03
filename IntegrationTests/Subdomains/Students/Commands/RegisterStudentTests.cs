using AcademicGateway.Application.Features.Students.Commands.RegisterStudent;
using AcademicGateway.Domain.Curriculum;
using AcademicGateway.Domain.Skills;
using AcademicGateway.Domain.Students;
using FluentAssertions;
using FluentValidation;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Infrastructure.Persistence.Context;

namespace AcademicGateway.IntegrationTests.Subdomains.Students.Commands;

/// <summary>
/// Integration tests verifying validation metrics, cross-aggregate relational constraints,
/// and profile provisioning side-effects inside the Student registration pipeline.
/// </summary>
[Collection("SharedDatabase")]
public class RegisterStudentTests : BaseIntegrationTest
{
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterStudentTests"/> class.
    /// </summary>
    /// <param name="factory">The centralized integration web application testing factory infrastructure context.</param>
    public RegisterStudentTests(CustomWebApplicationFactory factory) : base(factory)
    {
        _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    /// <summary>
    /// Ensures that executing a valid registration command correctly provisions both identity 
    /// tracking records and the linked, relational <see cref="Student"/> domain aggregate root.
    /// </summary>
    [Fact]
    public async Task Should_RegisterStudentAndCreateProfile_WhenCommandIsValid()
    {
        // --- 1. ARRANGE ---
        var major = new Major("Computer Science");
        major.AddSpecialty("Software Engineering");
        await AddAsync(major);

        var specialty = major.Specialties.Single(s => s.Name == "Software Engineering");

        var skill = new Skill("C# / .NET");
        await AddAsync(skill);

        var command = new RegisterStudentCommand
        {
            Email = "student.test@academicgateway.com",
            Username = "teststudent",
            Password = "SecurePassword123!",
            FullName = "Test Student",
            GraduationYear = 2026,
            MajorIds = new List<Guid> { major.Id },
            SpecialtyIds = new List<Guid> { specialty.Id },
            SkillIds = new List<Guid> { skill.Id }
        };

        // --- 2. ACT ---
        Guid studentId = await SendAsync(command);

        // --- 3. ASSERT ---
        studentId.Should().NotBeEmpty();

        // Query the database with explicit eager loading to populate relationship matrices
        Student? studentProfile;
        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            studentProfile = await context.Students
                .Include(s => s.StudentMajors)
                .Include(s => s.StudentSkills)
                .Include(s => s.StudentSpecialties)
                .FirstOrDefaultAsync(s => s.Id == studentId, TestContext.Current.CancellationToken);
        }

        studentProfile.Should().NotBeNull();
        studentProfile!.GraduationYear.Should().Be(command.GraduationYear);

        // Verify relationship consistency passes cleanly
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
            FullName = "",
            MajorIds = new List<Guid>()
        };

        // --- 2. ACT & 3. ASSERT ---
        Func<Task> act = async () => await SendAsync(invalidCommand);

        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.Any(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("required")))
            .Where(ex => ex.Errors.Any(e => e.PropertyName == "MajorIds" && e.ErrorMessage.Contains("at least one core academic Major program")));
    }

    /// <summary>
    /// Ensures that trying to map a specialty that does not belong to any of the 
    /// explicitly selected majors fails validation, throwing a <see cref="ValidationException"/>.
    /// </summary>
    [Fact]
    public async Task Should_ThrowValidationException_WhenSpecialtyDoesNotBelongToSelectedMajor()
    {
        // --- 1. ARRANGE ---
        var majorA = new Major("Computer Science");
        await AddAsync(majorA);

        var majorB = new Major("Mechanical Engineering");
        majorB.AddSpecialty("Thermodynamics");
        await AddAsync(majorB);

        var specialtyForB = majorB.Specialties.Single(s => s.Name == "Thermodynamics");

        var maliciousCommand = new RegisterStudentCommand
        {
            Email = "crossref.test@academicgateway.com",
            Username = "crossreftest",
            Password = "SecurePassword123!",
            FullName = "Cross Reference Student",
            MajorIds = new List<Guid> { majorA.Id },
            SpecialtyIds = new List<Guid> { specialtyForB.Id }
        };

        // --- 2. ACT & 3. ASSERT ---
        Func<Task> act = async () => await SendAsync(maliciousCommand);

        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.Any(e => e.PropertyName == "SpecialtyIds" && e.ErrorMessage.Contains("do not belong to your chosen academic majors")));
    }

    /// <summary>
    /// Ensures that trying to claim credentials that are already assigned to an existing profile 
    /// is caught and rejected at the identity layer, throwing an explicit exception.
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
            FullName = "First Entry Profile",
            MajorIds = new List<Guid> { major.Id }
        };

        var command2 = new RegisterStudentCommand
        {
            Email = "duplicate.test@academicgateway.com",
            Username = "sharedusername",
            Password = "SecondPassword123!",
            FullName = "Second Entry Profile",
            MajorIds = new List<Guid> { major.Id }
        };

        await SendAsync(command1);

        // --- 2. ACT & 3. ASSERT ---
        Func<Task> act = async () => await SendAsync(command2);

        // Realigned pattern matching to match your refactored string pattern exactly
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Student identity configuration failed*");
    }
}
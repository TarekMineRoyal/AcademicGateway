using AcademicGateway.Application.Features.Students.Commands.RegisterStudent;
using AcademicGateway.Domain.Curriculum;
using AcademicGateway.Domain.Skills;
using AcademicGateway.Domain.Students;
using AcademicGateway.Infrastructure.Identity;
using FluentAssertions;
using IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AcademicGateway.IntegrationTests.CrossCutting.Persistence;

/// <summary>
/// Integration tests verifying database persistence constraints and foreign key cascade rules
/// when core authentication identity profiles are removed from the system.
/// </summary>
[Collection("SharedDatabase")]
public class IdentityUserCascadeDeleteTests : BaseIntegrationTest
{
    private readonly IServiceScopeFactory _scopeFactory;

    public IdentityUserCascadeDeleteTests(CustomWebApplicationFactory factory) : base(factory)
    {
        _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    /// <summary>
    /// Ensures that purging an underlying ASP.NET Core Identity <see cref="ApplicationUser"/> 
    /// completely cascades to remove the domain <see cref="Student"/> profile and all related relationship records.
    /// </summary>
    [Fact]
    public async Task DeletingIdentityUser_ShouldCascadeDelete_StudentProfileAndAllJunctionRecords()
    {
        // --- 1. ARRANGE ---
        // Instantiate and seed lookups following rich domain aggregate patterns
        var major = new Major("Data Science");
        major.AddSpecialty("Deep Learning");
        await AddAsync(major);

        // Extract the nested child specialty tracked inside the major aggregate root boundary
        var specialty = major.Specialties.Single(s => s.Name == "Deep Learning");

        var skill = new Skill("PyTorch");
        await AddAsync(skill);

        // Prepare a valid registration command targeting the seeded data elements
        var registrationCommand = new RegisterStudentCommand
        {
            Email = "identity.cascade@academicgateway.com",
            Username = "identitycascade",
            Password = "SecurePassword123!",
            FullName = "Cascade Test Student", // Satisfies domain invariants preventing blank names
            GraduationYear = 2026,
            MajorIds = new List<Guid> { major.Id },
            SpecialtyIds = new List<Guid> { specialty.Id },
            SkillIds = new List<Guid> { skill.Id }
        };

        // Dispatch command to process authentication and profile creation pipelines
        Guid studentId = await SendAsync(registrationCommand);

        // Pre-assertion: Verify operational entity records exist in the database layers
        (await FindAsync<Student>(studentId)).Should().NotBeNull();
        (await FindAsync<StudentMajor>(studentId, major.Id)).Should().NotBeNull();
        (await FindAsync<StudentSkill>(studentId, skill.Id)).Should().NotBeNull();
        (await FindAsync<StudentSpecialty>(studentId, specialty.Id)).Should().NotBeNull();

        // --- 2. ACT ---
        // Purge parent ApplicationUser using standard ASP.NET Identity infrastructure handlers
        using (var scope = _scopeFactory.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var identityUser = await userManager.FindByIdAsync(studentId.ToString());
            identityUser.Should().NotBeNull();

            var deletionResult = await userManager.DeleteAsync(identityUser!);
            deletionResult.Succeeded.Should().BeTrue();
        }

        // --- 3. ASSERT ---
        // Verify core profile aggregate root was entirely deleted by the relational cascading trigger rules
        (await FindAsync<Student>(studentId)).Should().BeNull();

        // Verify deep relational junction entries are entirely wiped out, leaving no orphan reference footprints
        (await FindAsync<StudentMajor>(studentId, major.Id)).Should().BeNull();
        (await FindAsync<StudentSkill>(studentId, skill.Id)).Should().BeNull();
        (await FindAsync<StudentSpecialty>(studentId, specialty.Id)).Should().BeNull();
    }
}
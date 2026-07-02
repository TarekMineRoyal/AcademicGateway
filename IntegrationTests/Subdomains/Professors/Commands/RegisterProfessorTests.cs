using AcademicGateway.Application.Features.Professors.Commands.RegisterProfessor;
using AcademicGateway.Domain.Professors;
using FluentAssertions;
using FluentValidation;
using IntegrationTests.Infrastructure;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.IntegrationTests.Subdomains.Professors.Commands;

/// <summary>
/// Integration tests verifying validation rules, user provisioning side-effects, 
/// and core aggregate profile creation inside the Professor registration pipeline.
/// </summary>
[Collection("SharedDatabase")]
public class RegisterProfessorTests : BaseIntegrationTest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterProfessorTests"/> class.
    /// </summary>
    /// <param name="factory">The centralized integration web application testing factory infrastructure context.</param>
    public RegisterProfessorTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Ensures that executing a valid registration command successfully builds out both the identity records 
    /// and the matching relational <see cref="Professor"/> profile record inside the storage subsystem.
    /// </summary>
    [Fact]
    public async Task Should_RegisterProfessorAndCreateProfile_WhenCommandIsValid()
    {
        // --- 1. ARRANGE ---
        // Configure a complete registration payload meeting all mandatory structural field parameters
        var command = new RegisterProfessorCommand
        {
            Email = "prof.smith@academicgateway.com",
            Username = "profsmith",
            Password = "SecurePassword123!",
            FullName = "Professor John Smith",
            AcademicDepartment = "Computer Science",
            Rank = "Full Professor",
            MaxSupervisionCapacity = 5
        };

        // --- 2. ACT ---
        Guid professorId = await SendAsync(command);

        // --- 3. ASSERT ---
        professorId.Should().NotBeEmpty();

        // Query the store directly to verify aggregate mapping boundaries
        var professorProfile = await FindAsync<Professor>(professorId);
        professorProfile.Should().NotBeNull();

        // Verifies against the refactored 'Department' property on the domain entity
        professorProfile!.Department.Should().Be(command.AcademicDepartment);
    }

    /// <summary>
    /// Ensures that attempting to register an account with a blank academic department configuration
    /// is caught by fluent validation interceptors, throwing a <see cref="ValidationException"/>.
    /// </summary>
    [Fact]
    public async Task Should_ThrowValidationException_WhenDepartmentIsEmpty()
    {
        // --- 1. ARRANGE ---
        // Supply correct prerequisite metadata fields to isolate validation failure strictly to the blank department field
        var command = new RegisterProfessorCommand
        {
            Email = "prof.bad@academicgateway.com",
            Username = "profbad",
            Password = "SecurePassword123!",
            FullName = "Professor Alan Turing",
            AcademicDepartment = "", // Triggers mandatory field criteria check rules
            Rank = "Associate Professor",
            MaxSupervisionCapacity = 3
        };

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(command);

        // --- 3. ASSERT ---
        await act.Should().ThrowAsync<ValidationException>();
    }
}
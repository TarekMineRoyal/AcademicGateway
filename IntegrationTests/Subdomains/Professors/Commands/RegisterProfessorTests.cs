using AcademicGateway.Application.Features.Professors.Commands.RegisterProfessor;
using AcademicGateway.Domain.Professors;
using FluentAssertions;
using FluentValidation;
using IntegrationTests.Infrastructure;
using Xunit;

namespace AcademicGateway.IntegrationTests.Subdomains.Professors.Commands;

/// <summary>
/// Integration tests verifying validation rules, user provisioning side-effects, 
/// and core aggregate profile creation inside the Professor registration pipeline.
/// </summary>
[Collection("SharedDatabase")]
public class RegisterProfessorTests : BaseIntegrationTest
{
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
        var command = new RegisterProfessorCommand
        {
            Email = "prof.smith@academicgateway.com",
            Username = "profsmith",
            Password = "SecurePassword123!",
            AcademicDepartment = "Computer Science"
        };

        // --- 2. ACT ---
        Guid professorId = await SendAsync(command);

        // --- 3. ASSERT ---
        professorId.Should().NotBeEmpty();

        // Query the store directly to verify aggregate mapping boundaries
        var professorProfile = await FindAsync<Professor>(professorId);
        professorProfile.Should().NotBeNull();

        // FIXES CS1061: Asserts against the refactored 'Department' property on the domain entity
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
        var command = new RegisterProfessorCommand
        {
            Email = "prof.bad@academicgateway.com",
            Username = "profbad",
            Password = "SecurePassword123!",
            AcademicDepartment = "" // Triggers mandatory field criteria check rules
        };

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(command);

        // --- 3. ASSERT ---
        await act.Should().ThrowAsync<ValidationException>();
    }
}
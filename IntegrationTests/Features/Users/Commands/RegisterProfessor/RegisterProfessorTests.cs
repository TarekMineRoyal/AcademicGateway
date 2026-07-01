using AcademicGateway.Application.Features.Users.Commands.RegisterProfessor;
using Domain.Professors;
using FluentAssertions;
using FluentValidation;
using Xunit;

namespace AcademicGateway.IntegrationTests.Features.Users.Commands.RegisterProfessor;

public class RegisterProfessorTests : BaseIntegrationTest
{
    public RegisterProfessorTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

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
        var userId = await SendAsync(command);

        // --- 3. ASSERT ---
        userId.Should().NotBeNullOrEmpty();

        // Query the database directly to verify the Professor record exists
        var professorProfile = await FindAsync<Professor>(userId);
        professorProfile.Should().NotBeNull();
        professorProfile!.AcademicDepartment.Should().Be(command.AcademicDepartment);
    }

    [Fact]
    public async Task Should_ThrowValidationException_WhenDepartmentIsEmpty()
    {
        // --- 1. ARRANGE ---
        var command = new RegisterProfessorCommand
        {
            Email = "prof.bad@academicgateway.com",
            Username = "profbad",
            Password = "SecurePassword123!",
            AcademicDepartment = "" // Assuming validator requires this field
        };

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(command);

        // --- 3. ASSERT ---
        await act.Should().ThrowAsync<ValidationException>();
    }
}
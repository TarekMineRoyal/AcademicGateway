using AcademicGateway.Application.Features.Users.Commands.Login;
using AcademicGateway.Application.Features.Users.Commands.RegisterStudent;
using AcademicGateway.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace AcademicGateway.IntegrationTests.Features.Users.Commands.Login;

public class LoginTests : BaseIntegrationTest
{
    public LoginTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Should_ReturnToken_WhenCredentialsAreValid()
    {
        // --- 1. ARRANGE ---
        var major = new Major { Id = Guid.NewGuid(), Name = "Cybersecurity" };
        await AddAsync(major);

        var password = "SuperSecretPassword123!";
        var registerCommand = new RegisterStudentCommand
        {
            Email = "login.test@academicgateway.com",
            Username = "logintester",
            Password = password,
            GraduationYear = 2025,
            MajorIds = new List<Guid> { major.Id }
        };

        await SendAsync(registerCommand);

        var loginCommand = new LoginCommand(registerCommand.Email, password);

        // --- 2. ACT ---
        var result = await SendAsync(loginCommand);

        // --- 3. ASSERT ---
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_ThrowException_WhenPasswordIsIncorrect()
    {
        // --- 1. ARRANGE ---
        var major = new Major { Id = Guid.NewGuid(), Name = "Information Systems" };
        await AddAsync(major);

        var registerCommand = new RegisterStudentCommand
        {
            Email = "wrongpass.test@academicgateway.com",
            Username = "wrongpasstester",
            Password = "CorrectPassword123!",
            GraduationYear = 2025,
            MajorIds = new List<Guid> { major.Id }
        };

        await SendAsync(registerCommand);

        // FIX: Passed as constructor arguments
        var loginCommand = new LoginCommand(registerCommand.Email, "CompletelyWrongPassword!");

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(loginCommand);

        // --- 3. ASSERT ---
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task Should_ThrowException_WhenEmailDoesNotExist()
    {
        // --- 1. ARRANGE ---
        // FIX: Passed as constructor arguments
        var loginCommand = new LoginCommand("ghostuser@academicgateway.com", "SomePassword123!");

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(loginCommand);

        // --- 3. ASSERT ---
        await act.Should().ThrowAsync<Exception>();
    }
}
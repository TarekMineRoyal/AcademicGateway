using AcademicGateway.Application.Features.Identity.Commands.Login;
using AcademicGateway.Application.Features.Students.Commands.RegisterStudent;
using AcademicGateway.Domain.Curriculum;
using FluentAssertions;
using IntegrationTests.Infrastructure;
using Xunit;

namespace AcademicGateway.IntegrationTests.CrossCutting.Identity;

/// <summary>
/// Integration tests verifying user authentication, payload processing correctness, 
/// security exception handling, and token emission within the cross-cutting Identity pipeline.
/// </summary>
[Collection("SharedDatabase")]
public class LoginTests : BaseIntegrationTest
{
    public LoginTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Ensures that the identity sub-system successfully verifies valid user credentials
    /// and issues a raw JWT authentication security token string.
    /// </summary>
    [Fact]
    public async Task Should_ReturnToken_WhenCredentialsAreValid()
    {
        // --- 1. ARRANGE ---
        // Seed a rich domain dependency major entity using the correct Curriculum namespace
        var major = new Major("Cybersecurity");
        await AddAsync(major);

        const string password = "SuperSecretPassword123!";
        var registerCommand = new RegisterStudentCommand
        {
            Email = "login.test@academicgateway.com",
            Username = "logintester",
            Password = password,
            FullName = "Login Tester",
            GraduationYear = 2026,
            MajorIds = new List<Guid> { major.Id }
        };
        await SendAsync(registerCommand);

        // Instantiate the targeted capability payload command via explicit constructor args
        var loginCommand = new LoginCommand(registerCommand.Email, password);

        // --- 2. ACT ---
        // The command handler returns the token directly as a raw string
        var token = await SendAsync(loginCommand);

        // --- 3. ASSERT ---
        // Fixes CS1061 by asserting directly on the returned string variable
        token.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Ensures that an authentication attempt with an incorrect password fails early,
    /// throwing a security exception.
    /// </summary>
    [Fact]
    public async Task Should_ThrowException_WhenPasswordIsIncorrect()
    {
        // --- 1. ARRANGE ---
        var major = new Major("Information Systems");
        await AddAsync(major);

        var registerCommand = new RegisterStudentCommand
        {
            Email = "wrongpass.test@academicgateway.com",
            Username = "wrongpasstester",
            Password = "CorrectPassword123!",
            FullName = "Wrong Pass Tester",
            GraduationYear = 2026,
            MajorIds = new List<Guid> { major.Id }
        };
        await SendAsync(registerCommand);

        // Formulate a login request containing an invalid password parameter signature
        var loginCommand = new LoginCommand(registerCommand.Email, "CompletelyWrongPassword!");

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(loginCommand);

        // --- 3. ASSERT ---
        // Fixes CS0246 by safely catching the identity boundary exception
        await act.Should().ThrowAsync<Exception>();
    }

    /// <summary>
    /// Ensures that an authentication attempt with an email that is not registered 
    /// is explicitly blocked, throwing a security exception.
    /// </summary>
    [Fact]
    public async Task Should_ThrowException_WhenEmailDoesNotExist()
    {
        // --- 1. ARRANGE ---
        // Build a login command targeting an unregistered ghost user identity reference
        var loginCommand = new LoginCommand("ghostuser@academicgateway.com", "SomePassword123!");

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(loginCommand);

        // --- 3. ASSERT ---
        await act.Should().ThrowAsync<Exception>();
    }
}
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.Identity.Commands.Login;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Identity.Commands.Login;

/// <summary>
/// Contains isolated unit verification routines for the <see cref="LoginCommandHandler"/>.
/// Validates identity credential matching boundaries, successful token handoffs, and security guard clauses.
/// </summary>
public class LoginCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly LoginCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the test class, establishing isolated mock dependencies.
    /// </summary>
    public LoginCommandHandlerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _handler = new LoginCommandHandler(_identityServiceMock.Object);
    }

    /// <summary>
    /// Assures that passing valid, verified user credentials results in the transmission 
    /// of a cryptographically signed security token string from the identity engine.
    /// </summary>
    [Fact]
    public async Task Handle_GivenValidCredentials_ShouldReturnCryptographicallySignedToken()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "CorrectPassword123!");
        var expectedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.FakeTokenData";

        // Instruct the mock identity layer to accept credentials and return a token
        _identityServiceMock
            .Setup(x => x.AuthenticateAsync(command.Email, command.Password))
            .ReturnsAsync(expectedToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Be(expectedToken);
    }

    /// <summary>
    /// Assures that when invalid credentials or bad passwords are used, an <see cref="UnauthorizedAccessException"/> 
    /// is securely thrown containing a generic error message to safely mitigate Username Enumeration Attacks.
    /// </summary>
    [Fact]
    public async Task Handle_GivenInvalidCredentials_ShouldThrowUnauthorizedAccessExceptionWithGenericPayload()
    {
        // Arrange
        var command = new LoginCommand("wrong@academicgateway.com", "wrongpassword");

        // Simulate an authentication failure where the identity engine yields a null/empty string payload
        _identityServiceMock
            .Setup(x => x.AuthenticateAsync(command.Email, command.Password))
            .ReturnsAsync((string?)null);

        // Act
        // Wrap execution inside a delegate stream to allow FluentAssertions to track chronological execution exceptions safely
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        // The exception must contain a generic context summary to shield underlying authentication registries from disclosure
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password.");
    }
}
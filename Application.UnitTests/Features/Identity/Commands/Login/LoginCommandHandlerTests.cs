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
/// Validates identity credential matching boundaries, successful token handoffs, 
/// responsive cancellation handling, and multi-branch security guard clause variations.
/// </summary>
public class LoginCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly LoginCommandHandler _handler;

    /// <summary>
    /// Initializes a pristine instance of the test class, establishing isolated mock dependencies.
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

        _identityServiceMock
            .Setup(x => x.AuthenticateAsync(command.Email, command.Password))
            .ReturnsAsync(expectedToken);

        // Act
        // Best Practice (xUnit1051): Pass TestContext.Current.CancellationToken to support responsive test run cancellations.
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Be(expectedToken);
    }

    /// <summary>
    /// Assures that when invalid credentials cause the identity tier to return null, 
    /// an <see cref="UnauthorizedAccessException"/> is thrown containing a generic error message 
    /// to mitigate Username Enumeration Attacks.
    /// </summary>
    [Fact]
    public async Task Handle_GivenCredentialsReturningNullToken_ShouldThrowUnauthorizedAccessExceptionWithGenericPayload()
    {
        // Arrange
        var command = new LoginCommand("wrong@academicgateway.com", "wrongpassword");

        _identityServiceMock
            .Setup(x => x.AuthenticateAsync(command.Email, command.Password))
            .ReturnsAsync((string?)null); // Failure Variation 1: Return Null reference tracking payload

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password.");
    }

    /// <summary>
    /// Assures that if the identity authentication layer returns an empty or whitespace token payload string,
    /// the handler's internal guard clause catches the invalid response and throws an <see cref="UnauthorizedAccessException"/>.
    /// Note: Input string is explicitly typed as nullable (string?) to suppress reference typing warning diagnostics.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("      ")]
    public async Task Handle_GivenCredentialsReturningBlankToken_ShouldThrowUnauthorizedAccessExceptionWithGenericPayload(string? blankToken)
    {
        // Arrange
        var command = new LoginCommand("badtoken@academicgateway.com", "anypassword");

        _identityServiceMock
            .Setup(x => x.AuthenticateAsync(command.Email, command.Password))
            .ReturnsAsync(blankToken!); // Failure Variation 2 & 3: Empty or whitespace text pass through

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password.");
    }
}
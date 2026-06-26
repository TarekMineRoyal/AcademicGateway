using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.Users.Commands.Login;
using FluentAssertions;
using Moq;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Users.Commands.Login;

public class LoginCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        // Login only requires the IdentityService!
        _identityServiceMock = new Mock<IIdentityService>();
        _handler = new LoginCommandHandler(_identityServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Token_When_Credentials_Are_Valid()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "CorrectPassword123!");
        var expectedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.FakeTokenData";

        // Setup the mock to return our fake token when given the exact right credentials
        _identityServiceMock
            .Setup(x => x.AuthenticateAsync(command.Email, command.Password))
            .ReturnsAsync(expectedToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedToken);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_Credentials_Are_Invalid()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "WrongPassword!");

        // Setup the mock to return null (simulating a failed login)
        _identityServiceMock
            .Setup(x => x.AuthenticateAsync(command.Email, command.Password))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
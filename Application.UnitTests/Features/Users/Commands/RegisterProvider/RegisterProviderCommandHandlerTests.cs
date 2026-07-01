using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.Users.Commands.RegisterProvider;
using Domain.Providers;
using FluentAssertions;
using MockQueryable.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Users.Commands.RegisterProvider;

public class RegisterProviderCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly RegisterProviderCommandHandler _handler;

    public RegisterProviderCommandHandlerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _dbContextMock = new Mock<IApplicationDbContext>();

        // Setup the mock for the Providers table
        var mockProvidersDbSet = new List<Provider>().BuildMockDbSet();
        _dbContextMock.Setup(db => db.Providers).Returns(mockProvidersDbSet.Object);

        _handler = new RegisterProviderCommandHandler(_identityServiceMock.Object, _dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_UserId_And_Set_Unverified_On_Successful_Registration()
    {
        // Arrange
        var command = new RegisterProviderCommand
        {
            Username = "techcorp",
            Email = "contact@techcorp.com",
            Password = "Password123!",
            OrganizationName = "Tech Corp",
            Industry = "Software",
            WebsiteUrl = "https://techcorp.com"
        };

        var expectedUserId = Guid.NewGuid().ToString();

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((true, expectedUserId, new List<string>()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedUserId);

        // Verify the entity was added with the correct data AND the default unverified state
        _dbContextMock.Verify(x => x.Providers.Add(It.Is<Provider>(p =>
            p.UserId == expectedUserId &&
            p.OrganizationName == command.OrganizationName &&
            p.IsVerified == false)), Times.Once);

        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_Exception_When_Identity_Creation_Fails()
    {
        // Arrange
        var command = new RegisterProviderCommand
        {
            Username = "techcorp",
            Email = "contact@techcorp.com",
            Password = "Password123!"
        };

        var errors = new List<string> { "Username already exists" };

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((false, string.Empty, errors));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Provider creation failed*Username already exists*");

        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
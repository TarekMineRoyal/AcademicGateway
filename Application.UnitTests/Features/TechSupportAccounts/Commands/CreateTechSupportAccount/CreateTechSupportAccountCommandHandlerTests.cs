using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.TechSupportAccounts.Commands.CreateTechSupportAccount;
using AcademicGateway.Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.TechSupportAccounts.Commands.CreateTechSupportAccount;

public class CreateTechSupportAccountCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IIdentityService> _mockIdentityService;
    private readonly CreateTechSupportAccountCommandHandler _handler;

    public CreateTechSupportAccountCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockIdentityService = new Mock<IIdentityService>();
        _handler = new CreateTechSupportAccountCommandHandler(_mockContext.Object, _mockIdentityService.Object);
    }

    [Fact]
    public async Task Handle_ValidVerifiedProvider_ShouldCreateIdentityAndTrackingEntity()
    {
        // Arrange
        var providerId = "auth0|verified_provider_123";
        var identityUserId = "identity-guid-9999";
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = providerId,
            Email = "support@provider.com",
            Password = "SecurePassword123!",
            FullName = "Alex Technical Support"
        };

        // Instantiate a verified Provider via reflection to match domain rules
        var provider = CreateEntityWithPrivateConstructor<Provider>();
        SetPrivateProperty(provider, nameof(Provider.UserId), providerId);
        SetPrivateProperty(provider, nameof(Provider.IsVerified), true);

        var mockProvidersSet = new List<Provider> { provider }.BuildMockDbSet();
        _mockContext.Setup(c => c.Providers).Returns(mockProvidersSet.Object);

        // Mock empty tech support list initialization
        var mockTechSupportSet = new List<TechSupportAccount>().BuildMockDbSet();
        _mockContext.Setup(c => c.TechSupportAccounts).Returns(mockTechSupportSet.Object);

        // Mock a successful Identity user generation
        _mockIdentityService.Setup(i => i.CreateUserAsync(command.Email, command.Email, command.Password))
            .ReturnsAsync((true, identityUserId, Array.Empty<string>()));

        // Act
        var resultId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultId.Should().NotBeEmpty();

        _mockContext.Verify(c => c.TechSupportAccounts.Add(It.Is<TechSupportAccount>(t =>
            t.ProviderId == providerId &&
            t.IdentityUserId == identityUserId &&
            t.FullName == command.FullName
        )), Times.Once);

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProviderDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = "auth0|non_existent_provider",
            Email = "support@test.com",
            Password = "Password!",
            FullName = "No Provider"
        };

        var mockProvidersSet = new List<Provider>().BuildMockDbSet();
        _mockContext.Setup(c => c.Providers).Returns(mockProvidersSet.Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*Provider profile with ID '{command.ProviderId}' was not found.*");

        _mockIdentityService.Verify(i => i.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ProviderIsNotVerified_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var providerId = "auth0|unverified_provider";
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = providerId,
            Email = "support@test.com",
            Password = "Password!",
            FullName = "Unverified Support"
        };

        var provider = CreateEntityWithPrivateConstructor<Provider>();
        SetPrivateProperty(provider, nameof(Provider.UserId), providerId);
        SetPrivateProperty(provider, nameof(Provider.IsVerified), false);

        var mockProvidersSet = new List<Provider> { provider }.BuildMockDbSet();
        _mockContext.Setup(c => c.Providers).Returns(mockProvidersSet.Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Unverified providers are not permitted to provision technical support accounts.");

        _mockIdentityService.Verify(i => i.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_IdentityServiceFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var providerId = "auth0|verified_provider";
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = providerId,
            Email = "bad-email@",
            Password = "123",
            FullName = "Failed Support"
        };

        var provider = CreateEntityWithPrivateConstructor<Provider>();
        SetPrivateProperty(provider, nameof(Provider.UserId), providerId);
        SetPrivateProperty(provider, nameof(Provider.IsVerified), true);

        var mockProvidersSet = new List<Provider> { provider }.BuildMockDbSet();
        _mockContext.Setup(c => c.Providers).Returns(mockProvidersSet.Object);

        var mockTechSupportSet = new List<TechSupportAccount>().BuildMockDbSet();
        _mockContext.Setup(c => c.TechSupportAccounts).Returns(mockTechSupportSet.Object);

        // Mock identity creation failure payload
        var executionErrors = new[] { "Password is too weak", "Email format invalid" };
        _mockIdentityService.Setup(i => i.CreateUserAsync(command.Email, command.Email, command.Password))
            .ReturnsAsync((false, string.Empty, executionErrors));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Failed to provision identity credentials: Password is too weak, Email format invalid");

        _mockContext.Verify(c => c.TechSupportAccounts.Add(It.IsAny<TechSupportAccount>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static T CreateEntityWithPrivateConstructor<T>() =>
        (T)Activator.CreateInstance(typeof(T), true)!;

    private static void SetPrivateProperty(object obj, string propertyName, object value) =>
        obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?
           .SetValue(obj, value);
}
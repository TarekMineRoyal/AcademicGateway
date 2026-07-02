using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.Providers.Commands.RegisterProvider;
using AcademicGateway.Domain.Providers;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Providers.Commands.RegisterProvider;

/// <summary>
/// Contains isolated unit verification routines for the <see cref="RegisterProviderCommandHandler"/>.
/// Validates identity credential handoffs, default unverified profile assignments, and transactional persistence.
/// </summary>
public class RegisterProviderCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly RegisterProviderCommandHandler _handler;

    /// <summary>
    /// Initializes a pristine instance of the test class, setting up isolated provider tracking mocks.
    /// </summary>
    public RegisterProviderCommandHandlerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _dbContextMock = new Mock<IApplicationDbContext>();

        // Setup the mock for the Providers table to simulate an empty relational data sequence baseline
        var mockProvidersDbSet = new List<Provider>().BuildMockDbSet();
        _dbContextMock.Setup(db => db.Providers).Returns(mockProvidersDbSet.Object);

        _handler = new RegisterProviderCommandHandler(_identityServiceMock.Object, _dbContextMock.Object);
    }

    /// <summary>
    /// Assures that a valid provider registration command successfully extracts a unique user Guid from 
    /// the identity system, allocates a matching unverified domain profile, and saves the changes.
    /// </summary>
    [Fact]
    public async Task Handle_GivenValidRegistrationDetails_ShouldReturnUserIdAndPersistUnverifiedProfile()
    {
        // Arrange
        var command = new RegisterProviderCommand
        {
            Username = "techcorp",
            Email = "contact@techcorp.com",
            Password = "Password123!",
            CompanyName = "Tech Corp"
        };

        var expectedUserId = Guid.NewGuid();

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((true, expectedUserId, new List<string>()));

        // Act
        // Best Practice (xUnit1051): Pass TestContext.Current.CancellationToken for responsive test abort controls.
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(expectedUserId);

        // Verify the entity was mapped, initialized to an unverified standing state, and queued into the tracking context
        _dbContextMock.Verify(x => x.Providers.Add(It.Is<Provider>(p =>
            p.Id == expectedUserId &&
            p.CompanyName == command.CompanyName &&
            p.IsVerified == false)), Times.Once);

        _dbContextMock.Verify(x => x.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that if credential provisioning fails at the infrastructure identity tier,
    /// an <see cref="InvalidOperationException"/> is thrown and data modifications are entirely aborted.
    /// </summary>
    [Fact]
    public async Task Handle_GivenIdentityServiceFailure_ShouldThrowInvalidOperationExceptionAndAbortPersistence()
    {
        // Arrange
        var command = new RegisterProviderCommand
        {
            Username = "techcorp",
            Email = "contact@techcorp.com",
            Password = "Password123!",
            CompanyName = "Tech Corp"
        };

        var errors = new List<string> { "Username already exists" };

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((false, Guid.Empty, errors));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        // FIX: Synchronized wildcard string pattern to match the actual handler output
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Provider identity configuration failed*Username already exists*");

        // Guarantee that no tracking alterations or uncommitted records are flushed to persistence
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.Providers.Commands.RegisterProvider;
using AcademicGateway.Domain.Providers;
using AcademicGateway.Domain.Providers.Exceptions;
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
/// Validates identity credential handoffs, default unverified profile assignments, null-coalescing strings,
/// empty identity boundaries, and pass-through domain aggregate detail invariants.
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
            CompanyName = "Tech Corp",
            CompanyDescription = "A leading systems integration firm delivering scalable enterprise software tracks.",
            WebsiteUrl = "https://techcorp.com"
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
            p.CompanyDescription == command.CompanyDescription &&
            p.WebsiteUrl == command.WebsiteUrl &&
            p.IsVerified == false)), Times.Once);

        _dbContextMock.Verify(x => x.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that when <see cref="RegisterProviderCommand.WebsiteUrl"/> is passed as null, the handler's
    /// internal null-coalescing guard safely maps the configuration to an empty string instead of causing errors.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNullWebsiteUrl_ShouldCoalesceToEmptyStringSafely()
    {
        // Arrange
        var command = new RegisterProviderCommand
        {
            Username = "coalescecorp",
            Email = "contact@coalesce.io",
            Password = "Password123!",
            CompanyName = "Coalesce Solutions",
            CompanyDescription = "Enterprise consultation branch infrastructure profiles.",
            WebsiteUrl = null // Explicitly triggering the null branch condition pathway
        };

        var expectedUserId = Guid.NewGuid();

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((true, expectedUserId, new List<string>()));

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(expectedUserId);

        _dbContextMock.Verify(x => x.Providers.Add(It.Is<Provider>(p =>
            p.Id == expectedUserId &&
            p.WebsiteUrl == string.Empty)), Times.Once);

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
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Provider identity configuration failed*Username already exists*");

        // Guarantee that no tracking alterations or uncommitted records are flushed to persistence
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if the identity tier reports a successful profile configuration but yields an uninitialized empty 
    /// Guid tracking tracker, the core domain aggregate halts instantiation, throwing an <see cref="InvalidProviderDetailsException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenIdentitySucceedsWithEmptyGuid_ShouldPropagateInvalidProviderDetailsExceptionAndAbort()
    {
        // Arrange
        var command = new RegisterProviderCommand
        {
            Username = "emptyguidcorp",
            Email = "empty@corp.com",
            Password = "Password123!",
            CompanyName = "Malformed Entity Group"
        };

        // Violation Scenario: succeeded equals true but returned unique identity key is empty
        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((true, Guid.Empty, new List<string>()));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidProviderDetailsException>();
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if invalid, empty, or whitespace text arguments are submitted for the company name parameter,
    /// deep domain aggregate root invariants prevent construction, throwing an <see cref="InvalidProviderDetailsException"/>.
    /// Note: Input defined as string? to cleanly eliminate compiler reference type warnings.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("     ")]
    [InlineData(null)]
    public async Task Handle_GivenInvalidCompanyName_ShouldPropagateInvalidProviderDetailsExceptionAndAbort(string? invalidName)
    {
        // Arrange
        var command = new RegisterProviderCommand
        {
            Username = "badnameuser",
            Email = "badname@test.com",
            Password = "Password123!",
            CompanyName = invalidName!,
            CompanyDescription = "Valid textual description content summary parameters."
        };

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((true, Guid.NewGuid(), new List<string>()));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidProviderDetailsException>();
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if invalid, empty, or whitespace text arguments are submitted for the corporate description summary,
    /// deep domain aggregate root invariants prevent mutation, throwing an <see cref="InvalidProviderDetailsException"/>.
    /// Note: Input defined as string? to cleanly eliminate compiler reference type warnings.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" \n \t ")]
    [InlineData(null)]
    public async Task Handle_GivenInvalidCompanyDescription_ShouldPropagateInvalidProviderDetailsExceptionAndAbort(string? invalidDesc)
    {
        // Arrange
        var command = new RegisterProviderCommand
        {
            Username = "baddescuser",
            Email = "baddesc@test.com",
            Password = "Password123!",
            CompanyName = "Valid Corporate Name",
            CompanyDescription = invalidDesc!
        };

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((true, Guid.NewGuid(), new List<string>()));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidProviderDetailsException>();
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
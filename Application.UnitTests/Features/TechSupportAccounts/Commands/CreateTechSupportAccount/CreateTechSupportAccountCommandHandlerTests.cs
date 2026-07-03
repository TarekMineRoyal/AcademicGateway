using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.TechSupportAccounts.Commands.CreateTechSupportAccount;
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

namespace AcademicGateway.Application.UnitTests.Features.TechSupportAccounts.Commands.CreateTechSupportAccount;

/// <summary>
/// Contains isolated unit verification routines for the <see cref="CreateTechSupportAccountCommandHandler"/>.
/// Validates provider onboarding check gates, identity provider routing, identity identity boundary exceptions,
/// and deep pass-through domain aggregate invariant detail details constraints.
/// </summary>
public class CreateTechSupportAccountCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IIdentityService> _mockIdentityService;
    private readonly CreateTechSupportAccountCommandHandler _handler;

    /// <summary>
    /// Initializes a pristine instance of the test class with isolated dependency mocks.
    /// </summary>
    public CreateTechSupportAccountCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockIdentityService = new Mock<IIdentityService>();
        _handler = new CreateTechSupportAccountCommandHandler(_mockContext.Object, _mockIdentityService.Object);
    }

    /// <summary>
    /// Assures that a verified corporate provider can successfully provision a tech support asset,
    /// generating identity credentials and mapping structural auditing fields for database persistence.
    /// </summary>
    [Fact]
    public async Task Handle_GivenVerifiedProviderAndValidInputs_ShouldCreateIdentityAndPersistAccount()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var expectedIdentityUserId = Guid.NewGuid();

        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = providerId,
            Email = "support@provider.com",
            Password = "SecurePassword123!",
            StaffNumber = "EMP-998811",
            SupportTier = "Tier 2 Helpdesk"
        };

        var provider = new Provider(providerId, "Acme Solutions Group");
        provider.VerifyProfile();

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.TechSupportAccounts).Returns(new List<TechSupportAccount>().BuildMockDbSet().Object);

        _mockIdentityService
            .Setup(i => i.CreateUserAsync(command.Email, command.Email, command.Password))
            .ReturnsAsync((true, expectedIdentityUserId, new List<string>()));

        // Act
        var resultId = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        resultId.Should().Be(expectedIdentityUserId);

        _mockContext.Verify(c => c.TechSupportAccounts.Add(It.Is<TechSupportAccount>(t =>
            t.Id == expectedIdentityUserId &&
            t.StaffNumber == command.StaffNumber &&
            t.SupportTier == command.SupportTier &&
            t.IsActive == true
        )), Times.Once);

        _mockContext.Verify(c => c.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that attempting to provision a support agent through a missing or invalid managing provider tracking ID
    /// stops execution and throws a precise <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNonExistentProviderId_ShouldThrowKeyNotFoundExceptionAndAbort()
    {
        // Arrange
        var wrongProviderId = Guid.NewGuid();
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = wrongProviderId,
            Email = "support@test.com",
            Password = "Password!",
            StaffNumber = "EMP-001",
            SupportTier = "Tier 1 Helpdesk"
        };

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider>().BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*Provider profile with ID '{wrongProviderId}' was not found.*");

        _mockIdentityService.Verify(i => i.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if a corporate provider attempts to spawn secondary technical support agents before passing
    /// their own verification gates, processing is blocked and a <see cref="ProviderNotVerifiedException"/> is raised.
    /// </summary>
    [Fact]
    public async Task Handle_GivenUnverifiedProvider_ShouldThrowProviderNotVerifiedExceptionAndAbort()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = providerId,
            Email = "support@test.com",
            Password = "Password!",
            StaffNumber = "EMP-002",
            SupportTier = "Tier 1 Helpdesk"
        };

        var provider = new Provider(providerId, "Unverified Corporate Entity");

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<ProviderNotVerifiedException>();

        _mockIdentityService.Verify(i => i.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that when infrastructure validation boundaries reject a new user allocation,
    /// an <see cref="InvalidOperationException"/> is thrown and data mutations are aborted.
    /// </summary>
    [Fact]
    public async Task Handle_GivenIdentityServiceRegistrationFailure_ShouldThrowInvalidOperationExceptionAndAbort()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = providerId,
            Email = "malformed-address@",
            Password = "123",
            StaffNumber = "EMP-003",
            SupportTier = "Tier 3 Systems Admin"
        };

        var provider = new Provider(providerId, "Verified High-Security Firm");
        provider.VerifyProfile();

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.TechSupportAccounts).Returns(new List<TechSupportAccount>().BuildMockDbSet().Object);

        var executionErrors = new List<string> { "Password is too weak", "Email format invalid" };
        _mockIdentityService
            .Setup(i => i.CreateUserAsync(command.Email, command.Email, command.Password))
            .ReturnsAsync((false, Guid.Empty, executionErrors));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Failed to provision identity credentials: Password is too weak, Email format invalid");

        _mockContext.Verify(c => c.TechSupportAccounts.Add(It.IsAny<TechSupportAccount>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if identity service configuration generates success indicators but returns a blank Guid tracker,
    /// the domain constructor intercepts processing and bubbles up an <see cref="InvalidTechSupportDetailsException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenIdentitySucceedsWithEmptyGuid_ShouldPropagateInvalidTechSupportDetailsExceptionAndAbort()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = providerId,
            Email = "support@firm.com",
            Password = "Password123!",
            StaffNumber = "EMP-777",
            SupportTier = "Tier 1 Helpdesk"
        };

        var provider = new Provider(providerId, "Verified Security Provider");
        provider.VerifyProfile();

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.TechSupportAccounts).Returns(new List<TechSupportAccount>().BuildMockDbSet().Object);

        // Violation: Success indicator is true but returned tracker code resolves to Guid.Empty
        _mockIdentityService
            .Setup(i => i.CreateUserAsync(command.Email, command.Email, command.Password))
            .ReturnsAsync((true, Guid.Empty, new List<string>()));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidTechSupportDetailsException>()
            .WithMessage("*Identity User ID cannot be empty.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if empty or whitespace text is provided for staff identifier tracking arguments,
    /// the domain layer blocks construction, throwing an <see cref="InvalidTechSupportDetailsException"/>.
    /// Note: Parameter defined as string? to cleanly eliminate reference variable compiler check warnings.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("    ")]
    [InlineData(null)]
    public async Task Handle_GivenInvalidStaffNumber_ShouldPropagateInvalidTechSupportDetailsExceptionAndAbort(string? invalidStaffNumber)
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = providerId,
            Email = "support@firm.com",
            Password = "Password123!",
            StaffNumber = invalidStaffNumber!,
            SupportTier = "Tier 1 Helpdesk"
        };

        var provider = new Provider(providerId, "Verified Security Provider");
        provider.VerifyProfile();

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);

        _mockIdentityService
            .Setup(i => i.CreateUserAsync(command.Email, command.Email, command.Password))
            .ReturnsAsync((true, Guid.NewGuid(), new List<string>()));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidTechSupportDetailsException>()
            .WithMessage("*Staff number cannot be empty or whitespace.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Assures that if empty or whitespace text is provided for security tier category tracking arguments,
    /// the domain layer blocks construction, throwing an <see cref="InvalidTechSupportDetailsException"/>.
    /// Note: Parameter defined as string? to cleanly eliminate reference variable compiler check warnings.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" \t \n ")]
    [InlineData(null)]
    public async Task Handle_GivenInvalidSupportTier_ShouldPropagateInvalidTechSupportDetailsExceptionAndAbort(string? invalidSupportTier)
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = providerId,
            Email = "support@firm.com",
            Password = "Password123!",
            StaffNumber = "EMP-882200",
            SupportTier = invalidSupportTier!
        };

        var provider = new Provider(providerId, "Verified Security Provider");
        provider.VerifyProfile();

        _mockContext.Setup(c => c.Providers).Returns(new List<Provider> { provider }.BuildMockDbSet().Object);

        _mockIdentityService
            .Setup(i => i.CreateUserAsync(command.Email, command.Email, command.Password))
            .ReturnsAsync((true, Guid.NewGuid(), new List<string>()));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidTechSupportDetailsException>()
            .WithMessage("*Support tier assignment level cannot be empty or whitespace.*");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
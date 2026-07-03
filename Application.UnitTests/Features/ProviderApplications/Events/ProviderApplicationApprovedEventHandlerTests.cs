using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProviderApplications.Events;
using AcademicGateway.Domain.Providers;
using AcademicGateway.Domain.Providers.Events;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.ProviderApplications.Events;

/// <summary>
/// Technical unit testing suite validating the execution flows, domain aggregate mutations, 
/// and exceptional path handlers of the <see cref="ProviderApplicationApprovedEventHandler"/> class.
/// </summary>
public class ProviderApplicationApprovedEventHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly ProviderApplicationApprovedEventHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderApplicationApprovedEventHandlerTests"/> class, 
    /// configuring an isolated mock workspace for the application data tracking context boundaries.
    /// </summary>
    public ProviderApplicationApprovedEventHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new ProviderApplicationApprovedEventHandler(_contextMock.Object);
    }

    /// <summary>
    /// Validates the happy path execution scenario where an existing corporate provider matches the 
    /// arriving event identifier, asserting that the target profile transitions into a verified status.
    /// </summary>
    /// <param name="providerIdStr">The unique string representation of the target provider aggregate root user identity key.</param>
    /// <param name="companyName">The public structural name assigned to the test corporate aggregate instance.</param>
    /// <param name="scenario">A descriptive tracking summary defining the data permutation conditions for diagnostics.</param>
    [Theory]
    [InlineData("8ad957e0-855b-4b12-4ad4-288b47781042", "Global Sponsoring Technologies Ltd", "Standard corporate partner verification sequence.")]
    [InlineData("f1a067bb-c5bb-474d-966a-b28b6b23b499", "Academic Matchmaking Industries Inc", "Alternative partner onboarding pipeline track.")]
    public async Task HandleAsync_WhenProviderExists_ElevatesProfileStateToVerified(
        string? providerIdStr,
        string? companyName,
        string? scenario)
    {
        // Arrange
        Assert.NotNull(providerIdStr);
        Assert.NotNull(companyName);
        Assert.NotNull(scenario);

        Guid providerId = Guid.Parse(providerIdStr);

        // Respect domain encapsulation by initializing aggregate via native constructors
        Provider provider = new(providerId, companyName);
        Assert.False(provider.IsVerified, "The provider model must start out in an unverified state to accurately measure side effects.");

        List<Provider> providerList = new() { provider };

        // Fix: Call BuildMockDbSet directly on the IEnumerable List to resolve CS1061
        var dbSetMock = providerList.BuildMockDbSet();

        _contextMock.Setup(c => c.Providers).Returns(dbSetMock.Object);

        ProviderApplicationApprovedEvent domainEvent = new(providerId);

        // Act
        await _handler.HandleAsync(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(provider.IsVerified, "The event handler failed to trigger the target aggregate mutation routine to verify the provider profile.");
    }

    /// <summary>
    /// Assures that the handler triggers a structured <see cref="KeyNotFoundException"/> if the arriving 
    /// event identifier references a non-existent aggregate record context.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WhenProviderDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        Guid searchProviderId = Guid.NewGuid();
        Guid existingProviderId = Guid.NewGuid();

        Provider nonMatchingProvider = new(existingProviderId, "Mismatched Industry Sponsor");
        List<Provider> providerList = new() { nonMatchingProvider };

        // Fix: Call BuildMockDbSet directly on the IEnumerable List to resolve CS1061
        var dbSetMock = providerList.BuildMockDbSet();

        _contextMock.Setup(c => c.Providers).Returns(dbSetMock.Object);

        ProviderApplicationApprovedEvent domainEvent = new(searchProviderId);

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(act);
        Assert.Contains(searchProviderId.ToString(), exception.Message);
    }

    /// <summary>
    /// Validates the responsiveness of the internal persistence querying workflows to operational task abort signals, 
    /// ensuring cancellations bubble up effectively through the execution call stack.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WhenCancellationIsRequested_AbortsQueryExecutionFlow()
    {
        // Arrange
        Guid providerId = Guid.NewGuid();
        Provider provider = new(providerId, "Cancellation Sponsor Lab");
        List<Provider> providerList = new() { provider };

        // Fix: Call BuildMockDbSet directly on the IEnumerable List to resolve CS1061
        var dbSetMock = providerList.BuildMockDbSet();

        _contextMock.Setup(c => c.Providers).Returns(dbSetMock.Object);

        ProviderApplicationApprovedEvent domainEvent = new(providerId);

        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(domainEvent, cts.Token);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(act);
        Assert.False(provider.IsVerified, "The aggregate profile state must remain unverified if execution is aborted mid-flight.");
    }
}
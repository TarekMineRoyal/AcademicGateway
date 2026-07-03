using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Features.ProviderApplications.Events;
using AcademicGateway.Domain.Providers.Events;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.ProviderApplications.Events;

/// <summary>
/// Technical unit testing suite designed to validate the behavioral patterns, logging outputs, 
/// and exception resilience of the <see cref="ProviderApplicationSubmittedEventHandler"/> class.
/// </summary>
public class ProviderApplicationSubmittedEventHandlerTests
{
    private readonly Mock<ILogger<ProviderApplicationSubmittedEventHandler>> _loggerMock;
    private readonly ProviderApplicationSubmittedEventHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderApplicationSubmittedEventHandlerTests"/> class,
    /// establishing isolated mocking boundaries for infrastructure-level diagnostic dependencies.
    /// </summary>
    public ProviderApplicationSubmittedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<ProviderApplicationSubmittedEventHandler>>();
        _handler = new ProviderApplicationSubmittedEventHandler(_loggerMock.Object);
    }

    /// <summary>
    /// Validates the happy path and structural branch path conditions by processing various 
    /// event configurations, confirming that structural diagnostic telemetry is written exactly once.
    /// </summary>
    /// <param name="applicationIdStr">The string variation of the onboarding application Guid tracking target.</param>
    /// <param name="providerIdStr">The string variation of the corporate partner identifier account context.</param>
    /// <param name="scenario">A descriptive marker outlining the boundary scope of the given data permutation.</param>
    [Theory]
    [InlineData("aed957e0-855b-4b12-4ad4-288b47781042", "b1bf627f-1111-4b12-8ad4-288b47781043", "Standard corporate provider registration onboarding submission pool entry scenario.")]
    [InlineData("00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000", "Structural boundary verification using empty aggregate identity markers.")]
    public async Task HandleAsync_WithVariousValidEventPayloads_ExecutesWithoutFaultAndLogsContext(
        string? applicationIdStr,
        string? providerIdStr,
        string? scenario)
    {
        // Arrange
        Assert.NotNull(applicationIdStr);
        Assert.NotNull(providerIdStr);
        Assert.NotNull(scenario);

        Guid applicationId = Guid.Parse(applicationIdStr);
        Guid providerId = Guid.Parse(providerIdStr);

        ProviderApplicationSubmittedEvent domainEvent = new(applicationId, providerId);

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(
            domainEvent,
            TestContext.Current.CancellationToken);

        // Assert
        Exception? exception = await Record.ExceptionAsync(act);
        Assert.Null(exception);

        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, type) =>
                    state.ToString()!.Contains(applicationId.ToString()) &&
                    state.ToString()!.Contains(providerId.ToString())),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((state, exceptionType) => true)),
            Times.Once,
            "The handler failed to execute the structured logging protocol containing both the ApplicationId and ProviderId exactly once.");
    }

    /// <summary>
    /// Assures that the event handling execution framework respects structural task abort states 
    /// when thread execution cancellations are initiated.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WhenOperationalCancellationIsTriggered_RespectsAbortedContextStateFlows()
    {
        // Arrange
        Guid applicationId = Guid.NewGuid();
        Guid providerId = Guid.NewGuid();
        ProviderApplicationSubmittedEvent domainEvent = new(applicationId, providerId);

        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(domainEvent, cts.Token);

        // Assert
        Exception? exception = await Record.ExceptionAsync(act);

        if (exception is null)
        {
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, type) =>
                        state.ToString()!.Contains(applicationId.ToString()) &&
                        state.ToString()!.Contains(providerId.ToString())),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((state, exceptionType) => true)),
                Times.Once);
        }
        else
        {
            Assert.IsAssignableFrom<OperationCanceledException>(exception);
        }
    }

    /// <summary>
    /// Confirms that infrastructure telemetry exceptions do not get silently swallowed or hidden, 
    /// bubbling critical faults up to the host application process environment transparently.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WhenLoggingInfrastructureEncounterFaults_BubblesExceptionToHostProcess()
    {
        // Arrange
        Guid applicationId = Guid.NewGuid();
        Guid providerId = Guid.NewGuid();
        ProviderApplicationSubmittedEvent domainEvent = new(applicationId, providerId);

        _loggerMock.Setup(logger => logger.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Throws(new OutOfMemoryException("Diagnostics logging stack framework buffer array index mismatch error."));

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(
            domainEvent,
            TestContext.Current.CancellationToken);

        // Assert
        await Assert.ThrowsAsync<OutOfMemoryException>(act);
    }
}
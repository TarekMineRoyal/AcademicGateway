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
/// and exception resilience of the <see cref="ProviderApplicationRejectedEventHandler"/> class.
/// </summary>
public class ProviderApplicationRejectedEventHandlerTests
{
    private readonly Mock<ILogger<ProviderApplicationRejectedEventHandler>> _loggerMock;
    private readonly ProviderApplicationRejectedEventHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderApplicationRejectedEventHandlerTests"/> class,
    /// establishing isolated mocking boundaries for infrastructure-level diagnostic dependencies.
    /// </summary>
    public ProviderApplicationRejectedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<ProviderApplicationRejectedEventHandler>>();
        _handler = new ProviderApplicationRejectedEventHandler(_loggerMock.Object);
    }

    /// <summary>
    /// Validates the happy path and structural branch path conditions by processing various 
    /// event configurations, confirming that structured warning telemetry is written exactly once.
    /// </summary>
    /// <param name="applicationIdStr">The string variation of the onboarding application Guid tracking target.</param>
    /// <param name="providerIdStr">The string variation of the corporate partner identifier account context.</param>
    /// <param name="reviewerIdStr">The string representation of the evaluating compliance auditor identifier.</param>
    /// <param name="reason">The explanatory commentary framing why the documentation was deemed insufficient.</param>
    /// <param name="scenario">A descriptive marker outlining the boundary scope of the given data permutation.</param>
    [Theory]
    [InlineData("ead957e0-855b-4b12-4ad4-288b47781042", "f1bf627f-1111-4b12-8ad4-288b47781043", "a1c5d9e2-3333-4b12-8ad4-288b47781044", "Corporate legal registrations failed standard institutional verification thresholds.", "Standard provider application rejection tracking scenario.")]
    [InlineData("00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000", "", "Structural boundary verification using empty aggregate identity markers and a blank reason string.")]
    [InlineData("21a067bb-c5bb-474d-966a-b28b6b23b499", "351259d8-1923-41a2-b91c-7ccce455f8aa", "461259d8-1923-41a2-b91c-7ccce455f8bb", "   ", "Whitespace verification test case within event record payloads.")]
    public async Task HandleAsync_WithVariousValidEventPayloads_ExecutesWithoutFaultAndLogsWarning(
        string? applicationIdStr,
        string? providerIdStr,
        string? reviewerIdStr,
        string? reason,
        string? scenario)
    {
        // Arrange
        Assert.NotNull(applicationIdStr);
        Assert.NotNull(providerIdStr);
        Assert.NotNull(reviewerIdStr);
        Assert.NotNull(reason);
        Assert.NotNull(scenario);

        Guid applicationId = Guid.Parse(applicationIdStr);
        Guid providerId = Guid.Parse(providerIdStr);
        Guid reviewerId = Guid.Parse(reviewerIdStr);

        ProviderApplicationRejectedEvent domainEvent = new(applicationId, providerId, reviewerId, reason);

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(
            domainEvent,
            TestContext.Current.CancellationToken);

        // Assert
        Exception? exception = await Record.ExceptionAsync(act);
        Assert.Null(exception);

        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, type) =>
                    state.ToString()!.Contains(applicationId.ToString()) &&
                    state.ToString()!.Contains(reviewerId.ToString()) &&
                    state.ToString()!.Contains(reason)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((state, exceptionType) => true)),
            Times.Once,
            "The handler failed to execute the warning-level logging protocol containing the ApplicationId, ReviewerId, and Reason exactly once.");
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
        Guid reviewerId = Guid.NewGuid();
        string reason = "Cancellation Testing Onboarding Hold Pattern";
        ProviderApplicationRejectedEvent domainEvent = new(applicationId, providerId, reviewerId, reason);

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
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, type) =>
                        state.ToString()!.Contains(applicationId.ToString()) &&
                        state.ToString()!.Contains(reason)),
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
        Guid reviewerId = Guid.NewGuid();
        string reason = "Fault Injection Assessment Compliance Pipeline";
        ProviderApplicationRejectedEvent domainEvent = new(applicationId, providerId, reviewerId, reason);

        _loggerMock.Setup(logger => logger.Log(
            LogLevel.Warning,
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
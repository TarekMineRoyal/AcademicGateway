using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Features.ProjectTemplates.Events;
using AcademicGateway.Domain.ProjectTemplates.Events;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.ProjectTemplates.Events;

/// <summary>
/// Technical unit testing suite designed to validate the behavioral patterns, logging outputs, 
/// and exception resilience of the <see cref="ProjectTemplateRejectedPermanentlyEventHandler"/> class.
/// </summary>
public class ProjectTemplateRejectedPermanentlyEventHandlerTests
{
    private readonly Mock<ILogger<ProjectTemplateRejectedPermanentlyEventHandler>> _loggerMock;
    private readonly ProjectTemplateRejectedPermanentlyEventHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectTemplateRejectedPermanentlyEventHandlerTests"/> class,
    /// establishing isolated mocking boundaries for infrastructure-level diagnostic dependencies.
    /// </summary>
    public ProjectTemplateRejectedPermanentlyEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<ProjectTemplateRejectedPermanentlyEventHandler>>();
        _handler = new ProjectTemplateRejectedPermanentlyEventHandler(_loggerMock.Object);
    }

    /// <summary>
    /// Validates the happy path and structural branch path conditions by processing various 
    /// event configurations, confirming that critical compliance diagnostic telemetry is written exactly once.
    /// </summary>
    /// <param name="templateIdStr">The string variation of the template Guid tracking target.</param>
    /// <param name="providerIdStr">The string variation of the corporate partner identifier.</param>
    /// <param name="reason">The administrative justification logged for executing the hard denial.</param>
    /// <param name="scenario">A descriptive marker outlining the boundary scope of the given data permutation.</param>
    [Theory]
    [InlineData("3ad957e0-855b-4b12-4ad4-288b47781042", "4bf627f1-1111-4b12-8ad4-288b47781043", "Proposed blueprint violates explicit corporate-institutional data sharing agreements.", "Standard permanent compliance rejection scenario.")]
    [InlineData("00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000", "", "Structural boundary verification using empty aggregate identity markers and a blank reason string.")]
    [InlineData("e1a067bb-c5bb-474d-966a-b28b6b23b499", "f51259d8-1923-41a2-b91c-7ccce455f8aa", "   ", "Whitespace verification test case within event record payloads.")]
    public async Task HandleAsync_WithVariousValidEventPayloads_ExecutesWithoutFaultAndLogsCriticalError(
        string? templateIdStr,
        string? providerIdStr,
        string? reason,
        string? scenario)
    {
        // Arrange
        Assert.NotNull(templateIdStr);
        Assert.NotNull(providerIdStr);
        Assert.NotNull(reason);
        Assert.NotNull(scenario);

        Guid templateId = Guid.Parse(templateIdStr);
        Guid providerId = Guid.Parse(providerIdStr);

        ProjectTemplateRejectedPermanentlyEvent domainEvent = new(templateId, providerId, reason);

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(
            domainEvent,
            TestContext.Current.CancellationToken);

        // Assert
        Exception? exception = await Record.ExceptionAsync(act);
        Assert.Null(exception);

        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, type) =>
                    state.ToString()!.Contains(templateId.ToString()) &&
                    state.ToString()!.Contains(reason)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((state, exceptionType) => true)),
            Times.Once,
            "The handler failed to execute the critical error logging protocol containing the TemplateId and Reason exactly once.");
    }

    /// <summary>
    /// Assures that the event handling execution framework respects structural task abort states 
    /// when thread execution cancellations are initiated.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WhenOperationalCancellationIsTriggered_RespectsAbortedContextStateFlows()
    {
        // Arrange
        Guid templateId = Guid.NewGuid();
        Guid providerId = Guid.NewGuid();
        string reason = "Cancellation Testing Terminal State Rejection";
        ProjectTemplateRejectedPermanentlyEvent domainEvent = new(templateId, providerId, reason);

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
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, type) =>
                        state.ToString()!.Contains(templateId.ToString()) &&
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
        Guid templateId = Guid.NewGuid();
        Guid providerId = Guid.NewGuid();
        string reason = "Fault Injection Assessment Baseline";
        ProjectTemplateRejectedPermanentlyEvent domainEvent = new(templateId, providerId, reason);

        _loggerMock.Setup(logger => logger.Log(
            LogLevel.Error,
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
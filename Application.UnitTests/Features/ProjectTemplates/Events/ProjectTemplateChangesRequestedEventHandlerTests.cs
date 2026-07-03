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
/// Architectural unit testing suite designed to validate the behavioral patterns, logging outputs, 
/// and exception resilience of the <see cref="ProjectTemplateChangesRequestedEventHandler"/> class.
/// </summary>
public class ProjectTemplateChangesRequestedEventHandlerTests
{
    private readonly Mock<ILogger<ProjectTemplateChangesRequestedEventHandler>> _loggerMock;
    private readonly ProjectTemplateChangesRequestedEventHandler _handler;

    /// <summary>
    /// Instantiates a new instance of the <see cref="ProjectTemplateChangesRequestedEventHandlerTests"/> class,
    /// establishing isolated mocking boundaries for infrastructure level diagnostic dependencies.
    /// </summary>
    public ProjectTemplateChangesRequestedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<ProjectTemplateChangesRequestedEventHandler>>();
        _handler = new ProjectTemplateChangesRequestedEventHandler(_loggerMock.Object);
    }

    /// <summary>
    /// Validates the happy path and structural branch path conditions by processing various 
    /// event configurations, confirming that structural diagnostic telemetry is written exactly once.
    /// </summary>
    /// <param name="templateIdStr">The string variation of the template Guid tracking target.</param>
    /// <param name="providerIdStr">The string variation of the corporate partner identifier.</param>
    /// <param name="feedback">The instructional text commentary containing requested corrections.</param>
    /// <param name="scenario">A descriptive marker outlining the boundary scope of the given data permutation.</param>
    [Theory]
    [InlineData("7ad957e0-855b-4b12-4ad4-288b47781042", "9bf627f1-1111-4b12-8ad4-288b47781043", "Title requires clear academic alignment parameters.", "Standard feedback parsing scenario.")]
    [InlineData("00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000", "", "Empty identifier boundaries and blank feedback properties.")]
    [InlineData("e1a067bb-c5bb-474d-966a-b28b6b23b499", "f51259d8-1923-41a2-b91c-7ccce455f8aa", "   ", "Whitespace validation check within event record payloads.")]
    public async Task HandleAsync_WithVariousValidEventPayloads_ExecutesWithoutFaultAndLogsContext(
        string? templateIdStr,
        string? providerIdStr,
        string? feedback,
        string? scenario)
    {
        // Arrange
        Assert.NotNull(templateIdStr);
        Assert.NotNull(providerIdStr);
        Assert.NotNull(feedback);
        Assert.NotNull(scenario);

        Guid templateId = Guid.Parse(templateIdStr);
        Guid providerId = Guid.Parse(providerIdStr);

        ProjectTemplateChangesRequestedEvent domainEvent = new(templateId, providerId, feedback);

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
                    state.ToString()!.Contains(templateId.ToString()) &&
                    state.ToString()!.Contains(providerId.ToString())),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((state, exceptionType) => true)),
            Times.Once,
            "The handler failed to execute the structured logging protocol containing both the TemplateId and ProviderId exactly once.");
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
        string feedback = "Context cancellation check.";
        ProjectTemplateChangesRequestedEvent domainEvent = new(templateId, providerId, feedback);

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
                    It.Is<It.IsAnyType>((state, type) => state.ToString()!.Contains(templateId.ToString())),
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
        string feedback = "Fault injection assessment.";
        ProjectTemplateChangesRequestedEvent domainEvent = new(templateId, providerId, feedback);

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
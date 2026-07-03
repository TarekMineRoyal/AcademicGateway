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
/// Technical unit testing suite validating the execution flows, side effects, 
/// and behavioral characteristics of the <see cref="ProjectTemplateApprovedEventHandler"/> aggregate lifecycle orchestrator.
/// </summary>
public class ProjectTemplateApprovedEventHandlerTests
{
    private readonly Mock<ILogger<ProjectTemplateApprovedEventHandler>> _loggerMock;
    private readonly ProjectTemplateApprovedEventHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectTemplateApprovedEventHandlerTests"/> class, 
    /// configuring isolated mock boundaries for dependent application-layer logging infrastructure.
    /// </summary>
    public ProjectTemplateApprovedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<ProjectTemplateApprovedEventHandler>>();
        _handler = new ProjectTemplateApprovedEventHandler(_loggerMock.Object);
    }

    /// <summary>
    /// Validates the happy path execution scenario where a certified domain event is received, 
    /// asserting that structural application diagnostics are written exactly once containing the target template identifier.
    /// </summary>
    /// <param name="templateIdStr">The string representation of the unique template target identifier.</param>
    /// <param name="providerIdStr">The string representation of the owning corporate provider identifier.</param>
    /// <param name="scenarioDescription">A descriptive marker detailing the unique test case permutation boundary for diagnostics alignment.</param>
    [Theory]
    [InlineData("4ad957e0-855b-4b12-4ad4-288b47781042", "6bf627f1-1111-4b12-8ad4-288b47781043", "Standard corporate partner template approval verification scenario.")]
    [InlineData("00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000", "Structural boundary verification utilizing empty aggregate identity markers.")]
    public async Task HandleAsync_WithValidEventPayload_ExecutesSuccessfullyAndLogsTelemetry(
        string? templateIdStr,
        string? providerIdStr,
        string? scenarioDescription)
    {
        // Arrange
        Assert.NotNull(templateIdStr);
        Assert.NotNull(providerIdStr);
        Assert.NotNull(scenarioDescription);

        Guid templateId = Guid.Parse(templateIdStr);
        Guid providerId = Guid.Parse(providerIdStr);

        ProjectTemplateApprovedEvent domainEvent = new(templateId, providerId);

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
                It.Is<It.IsAnyType>((state, type) => state.ToString()!.Contains(templateId.ToString())),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((state, exceptionType) => true)),
            Times.Once,
            "The handler failed to broadcast the mandatory structural lifecycle telemetry update containing the target TemplateId exactly once.");
    }

    /// <summary>
    /// Validates the robustness of the system when structural cancellations are bubbled up, 
    /// ensuring that the execution gracefully respects the provided operational thread cancellation boundaries.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WhenThreadCancellationIsRequested_ExecutesWithinAbortedContextHandledState()
    {
        // Arrange
        Guid templateId = Guid.NewGuid();
        Guid providerId = Guid.NewGuid();
        ProjectTemplateApprovedEvent domainEvent = new(templateId, providerId);

        using CancellationTokenSource canceledSource = new();
        await canceledSource.CancelAsync();

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(
            domainEvent,
            canceledSource.Token);

        // Assert
        Exception? exception = await Record.ExceptionAsync(act);

        // MediatR event handlers may optionally complete synchronously before an internal async checkpoint hits, 
        // or immediately bubble up an OperationCanceledException based on the host configuration context.
        // We ensure that if execution completes, state constraints remain fully un-mutated.
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
    /// Assures correct system diagnostic propagation behavior when internal logging infrastructure mechanisms fail, 
    /// verifying that infrastructural faults fail-fast and ripple out transparently.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WhenLoggingInfrastructureThrowsFault_PropagatesExceptionUninterrupted()
    {
        // Arrange
        Guid templateId = Guid.NewGuid();
        Guid providerId = Guid.NewGuid();
        ProjectTemplateApprovedEvent domainEvent = new(templateId, providerId);

        _loggerMock.Setup(logger => logger.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Throws(new InvalidOperationException("Critical infrastructure diagnostics system out of memory disk space."));

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(
            domainEvent,
            TestContext.Current.CancellationToken);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }
}
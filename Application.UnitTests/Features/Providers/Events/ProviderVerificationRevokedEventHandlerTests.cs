using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.Providers.Events;
using AcademicGateway.Domain.ProjectTemplates;
using AcademicGateway.Domain.ProjectTemplates.Enums;
using AcademicGateway.Domain.ProjectTemplates.Exceptions;
using AcademicGateway.Domain.Providers.Events;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Providers.Events;

/// <summary>
/// Technical unit testing suite validating the security containment cascades, logging outputs, 
/// and domain invariant boundaries of the <see cref="ProviderVerificationRevokedEventHandler"/> class.
/// </summary>
public class ProviderVerificationRevokedEventHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ILogger<ProviderVerificationRevokedEventHandler>> _loggerMock;
    private readonly ProviderVerificationRevokedEventHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderVerificationRevokedEventHandlerTests"/> class, 
    /// establishing isolated mock boundaries for application persistence and diagnostic infrastructure.
    /// </summary>
    public ProviderVerificationRevokedEventHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _loggerMock = new Mock<ILogger<ProviderVerificationRevokedEventHandler>>();
        _handler = new ProviderVerificationRevokedEventHandler(_contextMock.Object, _loggerMock.Object);
    }

    /// <summary>
    /// Validates the happy path cascade scenario where public-facing project blueprints matching the 
    /// unauthorized corporate partner are located in a state that permits adjustments, asserting 
    /// they are successfully quarantined and forced out of student matching pools.
    /// </summary>
    /// <param name="providerIdStr">The unique string representation of the frozen provider aggregate key.</param>
    /// <param name="title">The structural title allocated to the test blueprint aggregate root.</param>
    /// <param name="description">The overview requirement text assigned to the test blueprint.</param>
    /// <param name="scenario">A descriptive tracking summary defining the data permutation conditions for diagnostics.</param>
    [Theory]
    [InlineData("7ad957e0-855b-4b12-4ad4-288b47781042", "Distributed Ledger Clearing System Mockhouse", "Requires foundational knowledge of decentralized protocols and state consensus.", "Standard corporate compliance enforcement quarantine flow.")]
    [InlineData("c1a067bb-c5bb-474d-966a-b28b6b23b499", "Autonomous Drone Routing Grid Simulation", "Building algorithmic trajectory models over multi-agent spatial constraint arrays.", "Alternative corporate partner platform expulsion containment flow.")]
    public async Task HandleAsync_WhenActiveTemplatesExist_QuarantinesBlueprintsAndLogsAlerts(
        string? providerIdStr,
        string? title,
        string? description,
        string? scenario)
    {
        // Arrange
        Assert.NotNull(providerIdStr);
        Assert.NotNull(title);
        Assert.NotNull(description);
        Assert.NotNull(scenario);

        Guid providerId = Guid.Parse(providerIdStr);

        // Honor domain aggregate boundaries by invoking official lifecycle methods
        ProjectTemplate template = new(title, description, providerId);
        template.SubmitForReview(); // Elevates status to PendingReview, allowing RequestChanges containment to fire safely

        Assert.Equal(ProjectTemplateStatus.PendingReview, template.Status);

        List<ProjectTemplate> templatePool = new() { template };
        var dbSetMock = templatePool.BuildMockDbSet();
        _contextMock.Setup(c => c.ProjectTemplates).Returns(dbSetMock.Object);

        ProviderVerificationRevokedEvent domainEvent = new(providerId);

        // Act
        await _handler.HandleAsync(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(ProjectTemplateStatus.ChangesRequested, template.Status);
        Assert.Contains("quarantined", template.ReviewerFeedback, StringComparison.OrdinalIgnoreCase);

        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, type) => state.ToString()!.Contains(providerId.ToString())),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((state, exceptionType) => true)),
            Times.Once,
            "The handler failed to broadcast the security breach log entry with the targeted ProviderId exactly once.");

        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, type) => state.ToString()!.Contains("Isolating")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((state, exceptionType) => true)),
            Times.Once,
            "The handler failed to broadcast the quarantine trace log summarizing isolated counts exactly once.");
    }

    /// <summary>
    /// Verifies the branch path condition where a frozen provider profile possesses zero active project templates, 
    /// confirming clean execution and diagnostic output without any collection iteration loops.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WhenNoMatchingTemplatesExist_LogsSecurityBreachAndCompletesSafely()
    {
        // Arrange
        Guid providerId = Guid.NewGuid();
        Guid alternateProviderId = Guid.NewGuid();

        // Template belongs to a separate provider, so it must remain completely unmutated
        ProjectTemplate safeTemplate = new("Safe Baseline Application Lab", "Core software craftsmanship sandbox workspace.", alternateProviderId);
        safeTemplate.SubmitForReview();

        List<ProjectTemplate> templatePool = new() { safeTemplate };
        var dbSetMock = templatePool.BuildMockDbSet();
        _contextMock.Setup(c => c.ProjectTemplates).Returns(dbSetMock.Object);

        ProviderVerificationRevokedEvent domainEvent = new(providerId);

        // Act
        await _handler.HandleAsync(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(ProjectTemplateStatus.PendingReview, safeTemplate.Status);
        Assert.Null(safeTemplate.ReviewerFeedback);

        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, type) => state.ToString()!.Contains(providerId.ToString())),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((state, exceptionType) => true)),
            Times.Once);

        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    /// <summary>
    /// Validates pass-through domain aggregate exception bubble behavior when a project template exists 
    /// but occupies a lifecycle status that prevents modifications, asserting a fail-fast structural break.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WhenTemplateOccupiesInvalidStateForContainment_PropagatesDomainException()
    {
        // Arrange
        Guid providerId = Guid.NewGuid();

        // Initializing the model leaves it in Draft status. Call sequence skips .SubmitForReview()
        ProjectTemplate draftTemplate = new("Draft Pipeline System Blueprint", "Unsubmitted operational parameters layout compilation.", providerId);
        Assert.Equal(ProjectTemplateStatus.Draft, draftTemplate.Status);

        List<ProjectTemplate> templatePool = new() { draftTemplate };
        var dbSetMock = templatePool.BuildMockDbSet();
        _contextMock.Setup(c => c.ProjectTemplates).Returns(dbSetMock.Object);

        ProviderVerificationRevokedEvent domainEvent = new(providerId);

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        await Assert.ThrowsAsync<InvalidTemplateStatusException>(act);
        Assert.Equal(ProjectTemplateStatus.Draft, draftTemplate.Status); // Verify state encapsulation holds intact
    }

    /// <summary>
    /// Confirms that infrastructure execution cancellations are processed cleanly, immediately halting 
    /// operations when thread execution abort signals are triggered.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WhenCancellationIsRequested_AbortsDownstreamExecutionFlow()
    {
        // Arrange
        Guid providerId = Guid.NewGuid();
        ProjectTemplate template = new("Aborted Query Context Blueprint", "Asynchronous operation cancel evaluation track.", providerId);
        template.SubmitForReview();

        List<ProjectTemplate> templatePool = new() { template };
        var dbSetMock = templatePool.BuildMockDbSet();
        _contextMock.Setup(c => c.ProjectTemplates).Returns(dbSetMock.Object);

        ProviderVerificationRevokedEvent domainEvent = new(providerId);

        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(domainEvent, cts.Token);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(act);
        Assert.Equal(ProjectTemplateStatus.PendingReview, template.Status); // State changes must be completely blocked
    }
}
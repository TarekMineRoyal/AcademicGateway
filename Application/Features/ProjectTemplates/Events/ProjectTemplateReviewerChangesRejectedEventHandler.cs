using AcademicGateway.Application.Common.Interfaces;
using Domain.ProjectTemplates.Events;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Events;

/// <summary>
/// Logs iteration data or flags administrative monitors when a partner declines a reviewer's optimization layouts.
/// </summary>
public class ProjectTemplateReviewerChangesRejectedEventHandler(ILogger<ProjectTemplateReviewerChangesRejectedEventHandler> logger)
    : IDomainEventHandler<ProjectTemplateReviewerChangesRejectedEvent>
{
    /// <summary>
    /// Evaluates structural iteration conflicts and preserves tracking continuity parameters.
    /// </summary>
    /// <param name="domainEvent">The contextual validation disagreement occurrence metadata wrapper.</param>
    /// <param name="cancellationToken">A token to monitor cross-network system cancellations requests.</param>
    /// <returns>A completed asynchronous execution task.</returns>
    public async Task HandleAsync(ProjectTemplateReviewerChangesRejectedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogWarning(
            "Provider {ProviderId} explicitly declined reviewer alterations on template {TemplateId}. Reverting to baseline workspace layout.",
            domainEvent.ProviderId,
            domainEvent.TemplateId);

        // System Analysis Side Effect: Record conflict metrics or update pipeline performance indicators
        await Task.CompletedTask;
    }
}
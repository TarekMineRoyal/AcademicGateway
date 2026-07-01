using AcademicGateway.Application.Common.Interfaces;
using Domain.ProjectTemplates.Events;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Events;

/// <summary>
/// Coordinates collaborative hand-offs when an auditor modifies text boundaries and awaits partner sign-off.
/// </summary>
public class ProjectTemplateReviewerChangesProposedEventHandler(ILogger<ProjectTemplateReviewerChangesProposedEventHandler> logger)
    : IDomainEventHandler<ProjectTemplateReviewerChangesProposedEvent>
{
    /// <summary>
    /// Processes confirmation request events, flagging the provider context that approval holds are pending.
    /// </summary>
    /// <param name="domainEvent">The contextual hand-off tracker instance payload.</param>
    /// <param name="cancellationToken">An operational signal tracking execution cancellations.</param>
    /// <returns>A completed asynchronous execution task.</returns>
    public async Task HandleAsync(ProjectTemplateReviewerChangesProposedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Auditor optimized specifications directly on template {TemplateId}. Holding verification pipeline for Provider {ProviderId} manual sign-off.",
            domainEvent.TemplateId,
            domainEvent.ProviderId);

        // Notification Hook: Flag the provider's active dashboard header that a text optimization hold requires action
        await Task.CompletedTask;
    }
}
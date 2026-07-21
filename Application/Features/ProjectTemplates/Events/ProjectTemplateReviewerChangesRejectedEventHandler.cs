using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectTemplates.Events;
using Microsoft.Extensions.Logging;

namespace AcademicGateway.Application.Features.ProjectTemplates.Events;

/// <summary>
/// Logs iteration data when a partner declines a reviewer's optimization layouts and reverts to draft.
/// Purges the template from the AI Matchmaking Engine index.
/// </summary>
public class ProjectTemplateReviewerChangesRejectedEventHandler(
    IAiMatchmakingClient aiClient,
    ILogger<ProjectTemplateReviewerChangesRejectedEventHandler> logger)
    : IDomainEventHandler<ProjectTemplateReviewerChangesRejectedEvent>
{
    public async Task HandleAsync(ProjectTemplateReviewerChangesRejectedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogWarning(
            "Provider {ProviderId} explicitly declined reviewer alterations on template {TemplateId}. Reverting to baseline workspace layout.",
            domainEvent.ProviderId,
            domainEvent.TemplateId);

        await aiClient.DeleteProjectAsync(domainEvent.TemplateId, cancellationToken);
    }
}
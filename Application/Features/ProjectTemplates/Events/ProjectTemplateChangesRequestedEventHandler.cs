using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectTemplates.Events;
using Microsoft.Extensions.Logging;

namespace AcademicGateway.Application.Features.ProjectTemplates.Events;

/// <summary>
/// Directs feedback dispatch protocols when a reviewer shifts a blueprint layout into modification cycles.
/// Dispatches a delete purge to the AI Matchmaking Engine to ensure unapproved revisions remain unindexed.
/// </summary>
public class ProjectTemplateChangesRequestedEventHandler(
    IAiMatchmakingClient aiClient,
    ILogger<ProjectTemplateChangesRequestedEventHandler> logger)
    : IDomainEventHandler<ProjectTemplateChangesRequestedEvent>
{
    public async Task HandleAsync(ProjectTemplateChangesRequestedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Reviewer requested modifications on template {TemplateId}. Routing feedback notes downstream to Provider {ProviderId}.",
            domainEvent.TemplateId,
            domainEvent.ProviderId);

        await aiClient.DeleteProjectAsync(domainEvent.TemplateId, cancellationToken);
    }
}
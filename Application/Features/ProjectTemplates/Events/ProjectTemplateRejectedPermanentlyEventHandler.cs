using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectTemplates.Events;
using Microsoft.Extensions.Logging;

namespace AcademicGateway.Application.Features.ProjectTemplates.Events;

/// <summary>
/// Executes administrative cleanup, logging, and hard archiving when a blueprint fails institutional compliance completely.
/// Purges the template from the AI Matchmaking Engine index.
/// </summary>
public class ProjectTemplateRejectedPermanentlyEventHandler(
    IAiMatchmakingClient aiClient,
    ILogger<ProjectTemplateRejectedPermanentlyEventHandler> logger)
    : IDomainEventHandler<ProjectTemplateRejectedPermanentlyEvent>
{
    public async Task HandleAsync(ProjectTemplateRejectedPermanentlyEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogError(
            "CRITICAL LIFECYCLE REFUSAL: Project template {TemplateId} has been permanently denied by compliance staff. Reason: '{Reason}'",
            domainEvent.TemplateId,
            domainEvent.Reason);

        await aiClient.DeleteProjectAsync(domainEvent.TemplateId, cancellationToken);
    }
}
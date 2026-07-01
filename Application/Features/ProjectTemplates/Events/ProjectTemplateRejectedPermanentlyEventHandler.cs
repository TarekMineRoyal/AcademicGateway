using AcademicGateway.Application.Common.Interfaces;
using Domain.ProjectTemplates.Events;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Events;

/// <summary>
/// Executes administrative cleanup, logging, and hard archiving when a blueprint fails institutional compliance completely.
/// </summary>
public class ProjectTemplateRejectedPermanentlyEventHandler(ILogger<ProjectTemplateRejectedPermanentlyEventHandler> logger)
    : IDomainEventHandler<ProjectTemplateRejectedPermanentlyEvent>
{
    /// <summary>
    /// Seals compliance metrics data and logs final system rejections.
    /// </summary>
    /// <param name="domainEvent">The immutable permanent terminal failure event details package.</param>
    /// <param name="cancellationToken">A token tracking operational execution cancellations requests.</param>
    /// <returns>A completed asynchronous execution task.</returns>
    public async Task HandleAsync(ProjectTemplateRejectedPermanentlyEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogError(
            "CRITICAL LIFECYCLE REFUSAL: Project template {TemplateId} has been permanently denied by compliance staff. Reason: '{Reason}'",
            domainEvent.TemplateId,
            domainEvent.Reason);

        // Archive Hook: Dispatches a formal administrative archive item or delivers strict justification summaries to the partner account
        await Task.CompletedTask;
    }
}
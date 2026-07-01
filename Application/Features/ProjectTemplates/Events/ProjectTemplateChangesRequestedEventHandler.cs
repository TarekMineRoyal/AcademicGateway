using AcademicGateway.Application.Common.Interfaces;
using Domain.ProjectTemplates.Events;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Events;

/// <summary>
/// Directs feedback dispatch protocols when a reviewer shifts a blueprint layout into modification cycles.
/// </summary>
public class ProjectTemplateChangesRequestedEventHandler(ILogger<ProjectTemplateChangesRequestedEventHandler> logger)
    : IDomainEventHandler<ProjectTemplateChangesRequestedEvent>
{
    /// <summary>
    /// Routes constructive modification criteria back onto corporate provider interaction vectors.
    /// </summary>
    /// <param name="domainEvent">The event information housing specific auditor corrections.</param>
    /// <param name="cancellationToken">A token to track and process runtime cancellations requests.</param>
    /// <returns>An asynchronous tracking task representing the alert execution routing.</returns>
    public async Task HandleAsync(ProjectTemplateChangesRequestedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Reviewer requested modifications on template {TemplateId}. Routing feedback notes downstream to Provider {ProviderId}.",
            domainEvent.TemplateId,
            domainEvent.ProviderId);

        // Operational Side Effect: Deliver specific modification logs ('domainEvent.Feedback') back onto the partner workspace
        await Task.CompletedTask;
    }
}
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectTemplates.Events;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Events;

/// <summary>
/// Handles secondary, out-of-band application logic triggered when a project template is pushed into the review pool.
/// </summary>
/// <remarks>
/// This keeps the core submission handler isolated from notification engine logic or administrative dashboard updates.
/// </remarks>
public class ProjectTemplateSubmittedEventHandler(ILogger<ProjectTemplateSubmittedEventHandler> logger)
    : IDomainEventHandler<ProjectTemplateSubmittedEvent>
{
    /// <summary>
    /// Processes submission alerts and administrative notifications for pending faculty reviews.
    /// </summary>
    /// <param name="domainEvent">The contextual immutable submission event payload.</param>
    /// <param name="cancellationToken">A token to propagate operational cancellations.</param>
    /// <returns>A task tracking the asynchronous side-effect processing loop.</returns>
    public async Task HandleAsync(ProjectTemplateSubmittedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Project template {TemplateId} submitted by Provider {ProviderId} has entered the faculty review pool.",
            domainEvent.TemplateId,
            domainEvent.ProviderId);

        // Integration Hook: Trigger an integration message or insert an alert into the Reviewer's pending task queue
        await Task.CompletedTask;
    }
}
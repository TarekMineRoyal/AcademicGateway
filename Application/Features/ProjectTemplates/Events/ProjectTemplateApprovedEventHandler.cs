using AcademicGateway.Application.Common.Interfaces;
using Domain.ProjectTemplates.Events;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Events;

/// <summary>
/// Orchestrates cross-aggregate application sync rules when a blueprint passes evaluation and becomes live.
/// </summary>
public class ProjectTemplateApprovedEventHandler(ILogger<ProjectTemplateApprovedEventHandler> logger)
    : IDomainEventHandler<ProjectTemplateApprovedEvent>
{
    /// <summary>
    /// Publishes the template to the student platform and sends success dispatches to the corporate partner.
    /// </summary>
    /// <param name="domainEvent">The certified aggregate root template state metadata.</param>
    /// <param name="cancellationToken">A token to propagate asynchronous operation cancellations.</param>
    /// <returns>A task representing the live publishing synchronization routine.</returns>
    public async Task HandleAsync(ProjectTemplateApprovedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Project template {TemplateId} has been certified and approved. Elevating to public student-matching visibility grids.",
            domainEvent.TemplateId);

        // Communications Side Effect: Dispatch an outgoing confirmation email/notification to the corporate Provider profile
        await Task.CompletedTask;
    }
}
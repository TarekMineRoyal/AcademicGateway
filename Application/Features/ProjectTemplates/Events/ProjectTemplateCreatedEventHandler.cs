using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectTemplates.Events;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Events;

/// <summary>
/// Handles the application-tier side effects triggered whenever a new project template blueprint is drafted.
/// </summary>
/// <remarks>
/// This class handles in-process decoupling. It can be used to synchronize read-models, seed audit trails, 
/// or warm up specific data caches without contaminating the core creation transaction.
/// </remarks>
public class ProjectTemplateCreatedEventHandler(ILogger<ProjectTemplateCreatedEventHandler> logger)
    : IDomainEventHandler<ProjectTemplateCreatedEvent>
{
    /// <summary>
    /// Executes side effects corresponding to the template draft initialization event.
    /// </summary>
    /// <param name="domainEvent">The immutable event context payload capturing creation metrics.</param>
    /// <param name="cancellationToken">A token to propagate concurrent asynchronous cancellation requests.</param>
    /// <returns>A structured asynchronous execution task tracking side effect completion.</returns>
    public async Task HandleAsync(ProjectTemplateCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Project template draft initialized successfully. TemplateId: {TemplateId}, ProviderId: {ProviderId}, Title: '{Title}'",
            domainEvent.TemplateId,
            domainEvent.ProviderId,
            domainEvent.Title);

        // Operational Side Effect: Seed a localized audit log or tracking metrics if required by the business
        await Task.CompletedTask;
    }
}
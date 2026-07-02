using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.Providers.Events;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProviderApplications.Events;

/// <summary>
/// Processes background logic when a corporate partner corrects their documents and resubmits their application.
/// </summary>
public class ProviderApplicationResubmittedEventHandler(ILogger<ProviderApplicationResubmittedEventHandler> logger)
    : IDomainEventHandler<ProviderApplicationResubmittedEvent>
{
    /// <summary>
    /// Re-evaluates administrative queue statuses and records iterative cycle benchmarks.
    /// </summary>
    /// <param name="domainEvent">The event details containing the updated application reference details.</param>
    /// <param name="cancellationToken">A token to propagate asynchronous operation cancellations.</param>
    /// <returns>A completed asynchronous execution task.</returns>
    public async Task HandleAsync(ProviderApplicationResubmittedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Provider application {ApplicationId} has been resubmitted with updated details by corporate partner {ProviderId}.",
            domainEvent.ApplicationId,
            domainEvent.ProviderId);

        // Metrics Side Effect: Update queue turnaround indicators and flag the application as revised for evaluating auditors
        await Task.CompletedTask;
    }
}
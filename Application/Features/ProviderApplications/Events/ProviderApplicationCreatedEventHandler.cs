using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.Providers.Events;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProviderApplications.Events;

/// <summary>
/// Handles the application-tier side effects triggered whenever a new provider onboarding application draft is spawned.
/// </summary>
/// <remarks>
/// This handler decouples the core registration from secondary operations such as initializing onboarding analytics, 
/// seeding tracking funnels, or logging initial provider metadata creation.
/// </remarks>
public class ProviderApplicationCreatedEventHandler(ILogger<ProviderApplicationCreatedEventHandler> logger)
    : IDomainEventHandler<ProviderApplicationCreatedEvent>
{
    /// <summary>
    /// Executes side effects corresponding to the application draft initialization event.
    /// </summary>
    /// <param name="domainEvent">The immutable event payload containing aggregate tracking identifiers.</param>
    /// <param name="cancellationToken">A token to observe and propagate cancellation requests across concurrent operations.</param>
    /// <returns>A completed asynchronous execution task.</returns>
    public async Task HandleAsync(ProviderApplicationCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Provider onboarding application draft spawned. ApplicationId: {ApplicationId}, ProviderId: {ProviderId}",
            domainEvent.ApplicationId,
            domainEvent.ProviderId);

        // Operational Hook: Prime local caching grids or initialize conversion-funnel analytics data if necessary
        await Task.CompletedTask;
    }
}
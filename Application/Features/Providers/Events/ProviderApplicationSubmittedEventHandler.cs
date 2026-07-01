using AcademicGateway.Application.Common.Interfaces;
using Domain.Providers.Events;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Providers.Events;

/// <summary>
/// Handles secondary application logic triggered when a corporate partner pushes their compiled draft into the review pool.
/// </summary>
/// <remarks>
/// Keeps the core submission handler isolated from notification engine logic or administrative review dashboard push updates.
/// </remarks>
public class ProviderApplicationSubmittedEventHandler(ILogger<ProviderApplicationSubmittedEventHandler> logger)
    : IDomainEventHandler<ProviderApplicationSubmittedEvent>
{
    /// <summary>
    /// Processes submission alerts and populates reviewer workload queues.
    /// </summary>
    /// <param name="domainEvent">The contextual immutable submission event details payload.</param>
    /// <param name="cancellationToken">A token to monitor cross-network system cancellation requests.</param>
    /// <returns>A completed asynchronous execution task.</returns>
    public async Task HandleAsync(ProviderApplicationSubmittedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Provider onboarding application {ApplicationId} formally submitted by Provider {ProviderId} and is awaiting evaluation.",
            domainEvent.ApplicationId,
            domainEvent.ProviderId);

        // Integration Hook: Insert an active notification into the quality assurance compliance reviewer workspace 
        await Task.CompletedTask;
    }
}
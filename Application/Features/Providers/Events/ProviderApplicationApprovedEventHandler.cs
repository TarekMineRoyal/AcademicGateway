using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using Domain.Providers.Events;
using Microsoft.EntityFrameworkCore;

namespace AcademicGateway.Application.Features.Providers.Events;

/// <summary>
/// Handles the application-tier side effects triggered whenever a provider's onboarding 
/// application transitions into an approved state.
/// </summary>
/// <remarks>
/// This class implements <see cref="IDomainEventHandler{ProviderApplicationApprovedEvent}"/>, ensuring that
/// the cross-aggregate synchronization rules (elevating the corresponding provider profile status) 
/// run isolated from the orchestrating reviewer command flow.
/// </remarks>
public class ProviderApplicationApprovedEventHandler(IApplicationDbContext context)
    : IDomainEventHandler<ProviderApplicationApprovedEvent>
{
    /// <summary>
    /// Synchronizes the state of the parent <see cref="Domain.Providers.Provider"/> aggregate root 
    /// based on the approved application event payload.
    /// </summary>
    /// <param name="domainEvent">The immutable event payload containing the target provider's identifier context.</param>
    /// <param name="cancellationToken">A token to observe and propagate cancellation requests across asynchronous operations.</param>
    /// <exception cref="KeyNotFoundException">Thrown if the target provider domain profile cannot be found in persistence.</exception>
    public async Task HandleAsync(ProviderApplicationApprovedEvent domainEvent, CancellationToken cancellationToken)
    {
        // 1. Fetch the corresponding provider profile matching the application's unique ID reference
        var provider = await context.Providers
            .FirstOrDefaultAsync(p => p.Id == domainEvent.ProviderId, cancellationToken);

        if (provider == null)
        {
            throw new KeyNotFoundException($"Provider aggregate profile with tracking ID '{domainEvent.ProviderId}' was not found.");
        }

        // 2. Invoke the explicit domain behavior on the aggregate root to execute standard business constraints
        provider.VerifyProfile();
    }
}
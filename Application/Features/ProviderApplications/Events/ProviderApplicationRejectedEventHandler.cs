using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.Providers.Events;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProviderApplications.Events;

/// <summary>
/// Directs feedback dispatch channels when an auditor denies an onboarding registration entry.
/// </summary>
public class ProviderApplicationRejectedEventHandler(ILogger<ProviderApplicationRejectedEventHandler> logger)
    : IDomainEventHandler<ProviderApplicationRejectedEvent>
{
    /// <summary>
    /// Routes specific justification commentary out-of-band directly to the rejected organization.
    /// </summary>
    /// <param name="domainEvent">The permanent failure state event payload housing auditor justifications.</param>
    /// <param name="cancellationToken">A token tracking concurrent operational execution cancellations.</param>
    /// <returns>A completed asynchronous execution task.</returns>
    public async Task HandleAsync(ProviderApplicationRejectedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogWarning(
            "Provider application {ApplicationId} rejected by Auditor {ReviewerId}. Reason logged: '{Reason}'",
            domainEvent.ApplicationId,
            domainEvent.ReviewerId,
            domainEvent.Reason);

        // Communications Side Effect: Route an transactional compliance mail containing the precise 'domainEvent.Reason' 
        // to guide the partner on what paperwork needs correction on their workspace.
        await Task.CompletedTask;
    }
}
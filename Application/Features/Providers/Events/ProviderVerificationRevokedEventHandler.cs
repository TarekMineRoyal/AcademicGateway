using AcademicGateway.Application.Common.Interfaces;
using Domain.Providers.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Providers.Events;

/// <summary>
/// Executes critical cascade safety operations across adjacent sub-domains when a partner's authorization is stripped.
/// </summary>
public class ProviderVerificationRevokedEventHandler(
    IApplicationDbContext context,
    ILogger<ProviderVerificationRevokedEventHandler> logger)
    : IDomainEventHandler<ProviderVerificationRevokedEvent>
{
    /// <summary>
    /// Traps security revocations, fetching and processing downstream items owned by the frozen partner.
    /// </summary>
    /// <param name="domainEvent">The security boundary occurrence context payload containing the targeted partner ID.</param>
    /// <param name="cancellationToken">A token to observe and propagate asynchronous operation cancellations.</param>
    /// <returns>A task tracking the asynchronous synchronization adjustments.</returns>
    public async Task HandleAsync(ProviderVerificationRevokedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogCritical(
            "SECURITY BREACH BOUNDARY: Provider verification revoked for profile {ProviderId}. Initiating downstream containment cascade.",
            domainEvent.ProviderId);

        // 1. Fetch any public-facing project templates associated with this unauthorized provider
        var activeTemplates = await context.ProjectTemplates
            .Where(pt => pt.ProviderId == domainEvent.ProviderId)
            .ToListAsync(cancellationToken);

        if (activeTemplates.Any())
        {
            logger.LogWarning(
                "Isolating {Count} project blueprints owned by suspended Provider {ProviderId} to secure matching integrity.",
                activeTemplates.Count,
                domainEvent.ProviderId);

            foreach (var template in activeTemplates)
            {
                // Note: To remain absolutely pure, if the ProjectTemplate aggregate is expanded down the line with a 
                // specialized .Suspend() or .Unpublish() behavior, invoke it here. For containment, we leverage 
                // RequestChanges to safely eject it from live status pools and bind it to a tracking administrative note.
                template.RequestChanges("Corporate provider verification was revoked. Blueprint automatically quarantined.");
            }

            // Note: The orchestrating base unit of work will automatically capture these cascading state modifications 
            // since this handler fires inside the pre-commit SaveChangesAsync() cycle loop.
        }
    }
}
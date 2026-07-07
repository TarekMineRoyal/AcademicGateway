using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.TechSupportAccounts.Commands.DeactivateTechSupportAccount;

/// <summary>
/// Handles the transaction routine to process a <see cref="DeactivateTechSupportAccountCommand"/>.
/// Validates parent provider ownership boundaries, locates the target technician account,
/// triggers its internal deactivation domain logic, and flushes changes cleanly.
/// </summary>
public class DeactivateTechSupportAccountCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<DeactivateTechSupportAccountCommand>
{
    /// <summary>
    /// Processes the tech support account deactivation, enforces corporate ownership checks, and saves mutations.
    /// </summary>
    /// <param name="request">The structural data bundle carrying the target tech support identifier.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>A completed asynchronous execution task.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if the security session is unauthenticated, or the user does not own the target resource.</exception>
    public async Task Handle(DeactivateTechSupportAccountCommand request, CancellationToken cancellationToken)
    {
        // 1. Enforce active security session validation early before querying data pools
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to deactivate support accounts.");
        }

        Guid providerId = currentUserService.UserId.Value;

        // 2. Retrieve the tech support account from the relational tracking set
        var account = await context.TechSupportAccounts
            .FirstOrDefaultAsync(a => a.Id == request.TechSupportAccountId, cancellationToken);

        // 3. Prevent cross-tenant data modification or tampering via strict resource boundary evaluation
        // Verifies the technician account maps directly to the requesting parent corporate provider identity
        if (account == null || account.ProviderId != providerId)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested tech support account was not found or is outside your organizational boundary.");
        }

        // 4. Invoke domain-level encapsulated deactivation behavior to flip active states securely
        account.DeactivateAccount();

        // 5. Commit outstanding aggregate updates atomically down to the persistence layer
        await context.SaveChangesAsync(cancellationToken);
    }
}
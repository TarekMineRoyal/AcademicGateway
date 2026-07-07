using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Providers.Commands.UpdateProviderProfile;

/// <summary>
/// Handles the transaction routine to process an <see cref="UpdateProviderProfileCommand"/>.
/// Resolves the provider aggregate via the secure session execution context, updates corporate details, 
/// and flushes states cleanly.
/// </summary>
public class UpdateProviderProfileCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<UpdateProviderProfileCommand>
{
    /// <summary>
    /// Processes the provider profile update, executes domain mutations, and saves changes securely.
    /// </summary>
    /// <param name="request">The structural data bundle carrying the updated provider organizational details.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>A completed asynchronous execution task.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if the security session is unauthenticated or the underlying profile does not exist.</exception>
    public async Task Handle(UpdateProviderProfileCommand request, CancellationToken cancellationToken)
    {
        // 1. Enforce active security session validation early before querying data pools
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to update provider profile metrics.");
        }

        Guid providerId = currentUserService.UserId.Value;

        // 2. Retrieve the provider aggregate root from the database tracking context
        var provider = await context.Providers
            .FirstOrDefaultAsync(p => p.Id == providerId, cancellationToken);

        // 3. Prevent side-channel exposure or mapping vulnerabilities by using a uniform exception boundary
        if (provider == null)
        {
            throw new UnauthorizedAccessException("Access Denied: Authorized provider profile context was not found.");
        }

        // 4. Invoke domain-level encapsulated mutations to enforce invariant states
        provider.UpdateCompanyName(request.CompanyName);
        provider.UpdateProfileDetails(request.CompanyDescription, request.WebsiteUrl);

        // 5. Commit outstanding aggregate alterations atomically down to relational storage structures
        await context.SaveChangesAsync(cancellationToken);
    }
}
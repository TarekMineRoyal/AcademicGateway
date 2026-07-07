using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Providers.Queries.GetProviderProfile;

/// <summary>
/// Handles the execution of the <see cref="GetProviderProfileQuery"/> request.
/// Leverages optimized, untracked relational database projections to compile a profile snapshot securely.
/// </summary>
public class GetProviderProfileQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetProviderProfileQuery, ProviderProfileDto>
{
    /// <summary>
    /// Processes the provider profile query by selecting database records straight into a read-only presentation DTO securely.
    /// </summary>
    /// <param name="request">The structural query container tracking the targeted provider identifier.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>A data transfer object layout containing the matched provider's profile data.</returns>
    /// <exception cref="UnauthorizedAccessException">Uniformly thrown if session authentication fails, the resource is missing, or tenancy validation fails.</exception>
    public async Task<ProviderProfileDto> Handle(GetProviderProfileQuery request, CancellationToken cancellationToken)
    {
        // Enforce active security session validation early before executing database logic
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to query provider profiles.");
        }

        // Verify tenancy alignment: Users can only query their own specific corporate provider profile context
        if (request.ProviderId != currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested profile was not found, or you do not possess read authorization permissions.");
        }

        // Project the relational database records directly into the updated profile contract.
        var profile = await context.Providers
            .AsNoTracking()
            .Where(p => p.Id == request.ProviderId)
            .Select(p => new ProviderProfileDto
            {
                Id = p.Id,
                CompanyName = p.CompanyName,
                CompanyDescription = p.CompanyDescription,
                WebsiteUrl = p.WebsiteUrl,
                IsVerified = p.IsVerified
            })
            .FirstOrDefaultAsync(cancellationToken);

        // Validate presence boundaries uniformly to protect against resource scanning behaviors
        if (profile == null)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested profile was not found, or you do not possess read authorization permissions.");
        }

        return profile;
    }
}
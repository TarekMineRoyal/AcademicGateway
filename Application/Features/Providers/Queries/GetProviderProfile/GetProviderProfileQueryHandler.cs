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
/// Leverages optimized, untracked relational database projections to compile a profile snapshot of a corporate provider.
/// </summary>
public class GetProviderProfileQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetProviderProfileQuery, ProviderProfileDto>
{
    /// <summary>
    /// Processes the provider profile query by selecting database records straight into a read-only presentation DTO.
    /// </summary>
    /// <param name="request">The structural query container tracking the targeted provider identifier.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>A data transfer object layout containing the matched provider's profile data.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if no corporate provider record maps to the provided identifier.</exception>
    public async Task<ProviderProfileDto> Handle(GetProviderProfileQuery request, CancellationToken cancellationToken)
    {
        // 1. Project the relational database records directly into the updated profile contract.
        // Performance Optimization: Direct LINQ projections bypass entity tracking overhead.
        // Warning CS8619 Fix: Explicitly capturing the local projection as a nullable instance to respect FirstOrDefaultAsync capabilities safely.
        ProviderProfileDto? profile = await context.Providers
            .AsNoTracking()
            .Where(p => p.Id == request.ProviderId)
            .Select(p => new ProviderProfileDto
            {
                Id = p.Id,
                CompanyName = p.CompanyName,
                CompanyDescription = p.CompanyDescription, // Fixed CS1061: Pulling verified domain string block
                WebsiteUrl = p.WebsiteUrl,
                IsVerified = p.IsVerified
            })
            .FirstOrDefaultAsync(cancellationToken);

        // 2. Enforce explicit boundary validation if the requested profile does not exist
        if (profile == null)
        {
            throw new KeyNotFoundException($"Provider profile with tracking identity '{request.ProviderId}' was not found within the institutional directory.");
        }

        return profile;
    }
}
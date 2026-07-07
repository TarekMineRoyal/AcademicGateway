using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.Providers.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProviderApplications.Queries.GetPendingProviderApplications;

/// <summary>
/// Handles the execution of the <see cref="GetPendingProviderApplicationsQuery"/> request.
/// Compiles the operational onboarding queue of pending corporate registrations for authorized administrative reviewers securely.
/// </summary>
public class GetPendingProviderApplicationsQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetPendingProviderApplicationsQuery, IReadOnlyCollection<PendingProviderApplicationDto>>
{
    /// <summary>
    /// Processes the verification queue query by selecting pending application data straight into lightweight presentation DTO records securely.
    /// </summary>
    /// <param name="request">The incoming CQRS read model request container.</param>
    /// <param name="cancellationToken">Propagates notification that database network execution routines should be canceled.</param>
    /// <returns>A read-only collection containing immutable dto representations of unmatched pending provider application records.</returns>
    /// <exception cref="UnauthorizedAccessException">Uniformly thrown if session validation or explicit role checking parameters fail boundaries.</exception>
    public async Task<IReadOnlyCollection<PendingProviderApplicationDto>> Handle(
        GetPendingProviderApplicationsQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Enforce active session validation early before executing infrastructure or database paths
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to query administrative operational queues.");
        }

        // 2. Fast-Path Authorization Guard: Leverage the newly optimized in-memory claim verification.
        // This instantly eliminates database lookup overhead, ensuring O(1) complexity performance.
        if (!currentUserService.IsInRole("Reviewer"))
        {
            throw new UnauthorizedAccessException("Access Denied: You do not possess the required administrative privileges to view the registration queue.");
        }

        // 3. Project database records directly into the lightweight read contract.
        // Performance Optimization: Direct LINQ projections instruct EF Core to generate precise SQL JOIN statements,
        // bypass database change tracking overhead entirely, and prevent over-fetching raw binary fields.
        return await context.ProviderApplications
            .AsNoTracking()
            .Where(a => a.Status == ProviderApplicationStatus.PendingReview)
            .OrderBy(a => a.CreatedAt) // Oldest submissions prioritized first in processing cycles
            .Select(a => new PendingProviderApplicationDto
            {
                Id = a.Id,
                ProviderName = a.Provider != null ? a.Provider.CompanyName : "Unknown Corporate Entity",
                CredentialsSummary = a.CompanyDetails,
                SubmittedAt = a.CreatedAt,
                ContactEmail = "verification-pending@academicgateway.internal"
            })
            .ToListAsync(cancellationToken);
    }
}
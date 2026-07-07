using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectInstances.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.SupervisionRequests.Queries.GetPendingSupervisionRequests;

/// <summary>
/// Handles the execution of the <see cref="GetPendingSupervisionRequestsQuery"/> request.
/// Leverages optimized, untracked database projections to extract outstanding matchmaking invitations securely.
/// </summary>
public class GetPendingSupervisionRequestsQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetPendingSupervisionRequestsQuery, IReadOnlyCollection<PendingSupervisionRequestDto>>
{
    /// <summary>
    /// Processes the pending supervision invitations lookup query, applying strict multi-tenant context verification boundaries.
    /// </summary>
    /// <param name="request">The query container containing the target ProfessorId lookup key.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>A read-only sequence containing safe projection representations of matched pending invitations.</returns>
    /// <exception cref="UnauthorizedAccessException">Uniformly thrown if session validation or explicit role checking parameters fail boundaries.</exception>
    public async Task<IReadOnlyCollection<PendingSupervisionRequestDto>> Handle(
        GetPendingSupervisionRequestsQuery request,
        CancellationToken cancellationToken)
    {
        // Enforce active security session validation early before executing database logic
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to query operational queues.");
        }

        // Strict Domain-Level Verification: Prevent side-channel resource enumeration and BOLA.
        // If a user attempts to fetch tracking records belonging to a different professor profile, 
        // throw a uniform authorization exception to maintain consistent cross-layer protection.
        if (request.ProfessorId != currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested operational queue was not found, or you do not possess read authorization permissions.");
        }

        // Build an IQueryable data pathway targeting ONLY requests flagged as Pending for the validated supervisor.
        // Performance Optimization: Direct LINQ projections automatically emit targeted SQL JOIN expressions, 
        // avoiding wasteful tracking cache or excessive eager loading overhead.
        var query = context.SupervisionRequests
            .AsNoTracking()
            .Where(r => r.ProfessorId == request.ProfessorId && r.Status == SupervisionRequestStatus.Pending);

        // Project matched relational tables directly into lean data transfer contracts
        return await query
            .Select(r => new PendingSupervisionRequestDto
            {
                Id = r.Id,
                ProjectInstanceId = r.ProjectInstanceId,

                // Safe evaluation against nullable relational links using snapshot fields
                ProjectTitle = r.ProjectInstance != null ? r.ProjectInstance.TitleSnapshot : "Unknown Title",
                ProjectDescription = r.ProjectInstance != null ? r.ProjectInstance.DescriptionSnapshot : "No Description Provided",

                PitchText = r.PitchText,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
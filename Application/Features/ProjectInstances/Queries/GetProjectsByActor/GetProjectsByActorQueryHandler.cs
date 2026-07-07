using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectInstances.Queries.GetProjectsByActor;

/// <summary>
/// Handles the execution of the <see cref="GetProjectsByActorQuery"/> request.
/// Employs performance-optimized, non-tracking database queries to dynamically compile role-specific workspace dashboards.
/// </summary>
public class GetProjectsByActorQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetProjectsByActorQuery, IReadOnlyCollection<ActorProjectDto>>
{
    /// <summary>
    /// Processes the dynamic project workspace lookup, enforcing strict identity alignment boundaries before projecting rows.
    /// </summary>
    /// <param name="request">The query details specifying the target ActorId and the role dashboard to generate.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>A read-only sequence of summary dashboard rows tailored to the requested role perspective.</returns>
    /// <exception cref="UnauthorizedAccessException">Uniformly thrown if session validation or explicit actor role pairings fail boundaries.</exception>
    public async Task<IReadOnlyCollection<ActorProjectDto>> Handle(
        GetProjectsByActorQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Enforce active security session validation early before executing database logic
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to query operational queues.");
        }

        // 2. Strict Domain-Level Verification: Mitigate Broken Object Level Authorization (BOLA) and side-channel harvesting.
        // Enforce that the authenticated user context exactly matches the requested ActorId, and carries the targeted role token.
        bool identityMatchesActor = currentUserService.UserId == request.ActorId;
        bool userCarriesTargetRole = currentUserService.IsInRole(request.Role);

        if (!identityMatchesActor || !userCarriesTargetRole)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested operational queue was not found, or you do not possess read authorization permissions.");
        }

        // 3. Establish baseline non-tracking read-path query root against the workspaces collection
        var databaseQuery = context.ProjectInstances.AsNoTracking();

        // 4. Inject polymorphic data filtering paths mapping strictly to the relational ownership definitions inside ProjectInstance
        databaseQuery = request.Role switch
        {
            "Student" => databaseQuery.Where(pi => pi.StudentId == request.ActorId),
            "Professor" => databaseQuery.Where(pi => pi.SupervisorId == request.ActorId),
            "Provider" => databaseQuery.Where(pi => pi.ProviderId == request.ActorId),
            _ => throw new InvalidOperationException("Invariant Violation: Validated dashboard execution route encountered an unmapped role mapping sequence.")
        };

        // 5. Project matching columns directly into the dashboard overview layout without tracking overhead
        return await databaseQuery
            .Select(pi => new ActorProjectDto
            {
                Id = pi.Id,
                StudentId = pi.StudentId,
                SupervisorId = pi.SupervisorId,
                ProviderId = pi.ProviderId,
                Title = pi.TitleSnapshot,
                Description = pi.DescriptionSnapshot,
                Status = pi.Status,
                CreatedAt = pi.CreatedAt,
                EndDate = pi.EndDate,
                OverallGrade = pi.OverallGrade
            })
            .ToListAsync(cancellationToken);
    }
}
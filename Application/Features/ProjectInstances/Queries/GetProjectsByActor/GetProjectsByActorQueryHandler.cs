using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectInstances.Enums;
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
                // --- Retained Existing Properties ---
                Id = pi.Id,
                StudentId = pi.StudentId,
                SupervisorId = pi.SupervisorId,
                ProviderId = pi.ProviderId,
                Title = pi.TitleSnapshot,
                Description = pi.DescriptionSnapshot,
                Status = pi.Status,
                CreatedAt = pi.CreatedAt,
                EndDate = pi.EndDate,
                OverallGrade = pi.OverallGrade,

                // --- Extended Redesign Properties ---

                // 1. Resolve Company Name directly from Providers table subquery due to missing navigational path
                ProviderCompanyName = context.Providers
                    .Where(p => p.Id == pi.ProviderId)
                    .Select(p => p.CompanyName)
                    .FirstOrDefault() ?? string.Empty,

                // 2. Active Mentorship Mapping and Solo status calculation
                ProfessorId = pi.SupervisorId,
                ProfessorName = pi.Supervisor != null ? pi.Supervisor.FullName : null,
                IsSoloMode = pi.Status == ProjectInstanceStatus.Active && !pi.SupervisorId.HasValue,

                // 3. Current Milestone Tracking Telemetry (Captures active milestone title)
                CurrentMilestoneTitle = pi.LocalMilestones
                    .Where(m => m.Status == LocalMilestoneStatus.InProgress)
                    .Select(m => m.TitleSnapshot)
                    .FirstOrDefault(),

                // FIXED: Moved the (decimal?) cast INSIDE the Sum selector. 
                // This forces EF Core to use a nullable overload for the sum calculation, ensuring that if 
                // zero tasks match the predicate, a SQL NULL is safely returned and coalesced into 0m instead of throwing an unboxing crash.
                CurrentMilestoneProgress = pi.LocalMilestones
                    .Where(m => m.Status == LocalMilestoneStatus.InProgress)
                    .Select(m => m.LocalTasks
                        .Where(t => t.Status == LocalTaskStatus.Submitted || t.Status == LocalTaskStatus.Graded)
                        .Sum(t => (decimal?)t.Weight))
                    .FirstOrDefault() ?? 0m,

                // FIXED: Wrapped the absolute milestone progress weight formula in an outer (decimal?) expression cast.
                // This ensures that if a project contains no initialized milestones yet, the outer SQL SUM expression evaluates 
                // cleanly to a nullable decimal, which is then safely caught by the null-coalescing operator (?? 0m).
                TotalProjectProgress = pi.LocalMilestones.Sum(m =>
                    (decimal?)(m.WbsWeight * ((m.LocalTasks
                        .Where(t => t.Status == LocalTaskStatus.Submitted || t.Status == LocalTaskStatus.Graded)
                        .Sum(t => (decimal?)t.Weight) ?? 0m) / 100m))) ?? 0m,

                // 5. Matchmaking / Pending Supervision Tracking using professor navigation routes on supervision requests
                RequestedProfessorId = pi.SupervisionRequests
                    .Where(r => r.Status == SupervisionRequestStatus.Pending)
                    .Select(r => (Guid?)r.ProfessorId)
                    .FirstOrDefault(),

                RequestedProfessorName = pi.SupervisionRequests
                    .Where(r => r.Status == SupervisionRequestStatus.Pending)
                    .Select(r => r.Professor.FullName)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);
    }
}
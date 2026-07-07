using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.TechSupportProposals.Queries.GetTechSupportProposals;

/// <summary>
/// Handles the execution of the <see cref="GetTechSupportProposalsQuery"/> request.
/// Leverages optimized, untracked relational database checks to securely project corporate assistance offers.
/// </summary>
public class GetTechSupportProposalsQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetTechSupportProposalsQuery, IReadOnlyCollection<TechSupportProposalDto>>
{
    /// <summary>
    /// Processes the corporate proposals lookup query, executing proactive multi-tenant ownership checking boundaries.
    /// </summary>
    /// <param name="request">The query container containing the target ProjectInstanceId identifier key.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>A read-only sequence containing safe projection representations of matched corporate offers.</returns>
    /// <exception cref="UnauthorizedAccessException">Uniformly thrown if session validation or explicit aggregate ownership verification parameters fail boundaries.</exception>
    public async Task<IReadOnlyCollection<TechSupportProposalDto>> Handle(
        GetTechSupportProposalsQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Enforce active security session validation early before executing database logic
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to query operational queues.");
        }

        // 2. Fetch baseline authorization keys from the parent ProjectInstance aggregate workspace.
        // Performance Optimization: Grab only the lightweight identifier primitives via an anonymous projection.
        // This avoids tracking or hydrating large memory structures while verifying object-level tenancy.
        var workspaceAuthContext = await context.ProjectInstances
            .AsNoTracking()
            .Select(pi => new
            {
                pi.Id,
                pi.StudentId,
                pi.SupervisorId
            })
            .FirstOrDefaultAsync(pi => pi.Id == request.ProjectInstanceId, cancellationToken);

        // 3. Strict Domain-Level Verification: Prevent Broken Object Level Authorization (BOLA) and metadata harvesting.
        // Access is strictly restricted to the owner student or assigned supervisor who possess authority to review proposals.
        // Uniformly throw an exception to hide workspace presence from unauthorized actor scopes.
        bool isAuthorizedActor = workspaceAuthContext != null &&
            (workspaceAuthContext.StudentId == currentUserService.UserId ||
             workspaceAuthContext.SupervisorId == currentUserService.UserId);

        if (!isAuthorizedActor)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested project workspace record was not found, or you do not possess read authorization permissions.");
        }

        // 4. Project matched relational child records directly into the immutable data contracts.
        return await context.TechSupportProposals
            .AsNoTracking()
            .Where(p => p.ProjectInstanceId == request.ProjectInstanceId)
            .Select(p => new TechSupportProposalDto
            {
                Id = p.Id,
                ProjectInstanceId = p.ProjectInstanceId,
                TechSupportAccountId = p.TechSupportAccountId,
                Status = p.Status,
                RejectionReason = p.RejectionReason,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectInstances.Queries.GetMilestoneComments;

/// <summary>
/// Handles the execution of the <see cref="GetMilestoneCommentsQuery"/> request.
/// Employs optimized, untracked relational projections to pull and sort discussion feeds chronologically and securely.
/// </summary>
public class GetMilestoneCommentsQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetMilestoneCommentsQuery, List<MilestoneCommentDto>>
{
    /// <summary>
    /// Processes the detailed milestone comment lookups, flattening aggregate navigation collections via LINQ securely.
    /// </summary>
    /// <param name="request">The query container holding the parent project workspace and targeted local milestone lookup keys.</param>
    /// <param name="cancellationToken">Propagates notification that network execution operations should be canceled.</param>
    /// <returns>A chronologically sorted collection list containing all matched discussion comment records.</returns>
    /// <exception cref="UnauthorizedAccessException">Uniformly thrown if session authentication fails, the resource is missing, or tenancy validation fails.</exception>
    public async Task<List<MilestoneCommentDto>> Handle(GetMilestoneCommentsQuery request, CancellationToken cancellationToken)
    {
        // Enforce active security session validation early before executing database logic
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to query discussion streams.");
        }

        // Fetch the workspace access tenancy framework parameters from the database layer
        var projectTenancy = await context.ProjectInstances
            .AsNoTracking()
            .Where(pi => pi.Id == request.ProjectInstanceId)
            .Select(pi => new { pi.StudentId, pi.SupervisorId, pi.ProviderId })
            .FirstOrDefaultAsync(cancellationToken);

        // Validate presence boundaries and verify contextual user tenancy parameters uniformly.
        // Data visibility boundaries are strictly confined to the participating student, supervisor, or provider profiles.
        if (projectTenancy == null || (projectTenancy.StudentId != currentUserService.UserId &&
                                       projectTenancy.SupervisorId != currentUserService.UserId &&
                                       projectTenancy.ProviderId != currentUserService.UserId))
        {
            throw new UnauthorizedAccessException("Access Denied: The requested project workspace was not found, or you do not possess read authorization permissions.");
        }

        // Query base: Deactivate change-tracking for read-only optimization.
        // Flatten the encapsulated collections using double SelectMany projections down to the sub-child Comments entity lane.
        return await context.ProjectInstances
            .AsNoTracking()
            .Where(pi => pi.Id == request.ProjectInstanceId)
            .SelectMany(pi => pi.LocalMilestones)
            .Where(lm => lm.Id == request.LocalMilestoneId)
            .SelectMany(lm => lm.Comments)
            .OrderBy(c => c.CreatedAt) // Ensure the chat stream maps out sequentially over time
            .Select(c => new MilestoneCommentDto
            {
                Id = c.Id,
                LocalMilestoneId = c.LocalMilestoneId,
                AuthorId = c.AuthorId,
                AuthorIdentitySnapshot = c.AuthorIdentitySnapshot,
                Content = c.Content,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
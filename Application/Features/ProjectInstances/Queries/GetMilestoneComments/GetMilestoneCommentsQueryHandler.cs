using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectInstances.Queries.GetMilestoneComments;

/// <summary>
/// Handles the execution of the <see cref="GetMilestoneCommentsQuery"/> request.
/// Employs optimized, untracked relational projections to pull and sort discussion feeds chronologically.
/// </summary>
public class GetMilestoneCommentsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetMilestoneCommentsQuery, List<MilestoneCommentDto>>
{
    /// <summary>
    /// Processes the detailed milestone comment lookups, flattening aggregate navigation collections via LINQ.
    /// </summary>
    /// <param name="request">The query container holding the parent project workspace and targeted local milestone lookup keys.</param>
    /// <param name="cancellationToken">Propagates notification that network execution operations should be canceled.</param>
    /// <returns>A chronologically sorted collection list containing all matched discussion comment records.</returns>
    public async Task<List<MilestoneCommentDto>> Handle(GetMilestoneCommentsQuery request, CancellationToken cancellationToken)
    {
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
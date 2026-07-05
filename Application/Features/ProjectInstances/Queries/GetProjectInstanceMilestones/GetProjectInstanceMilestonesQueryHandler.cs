using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectInstances.Queries.GetProjectInstanceMilestones;

/// <summary>
/// Handles the execution of the <see cref="GetProjectInstanceMilestonesQuery"/> request.
/// Leverages untracked relational flattening queries to compile the running instance milestone checklist.
/// </summary>
public class GetProjectInstanceMilestonesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetProjectInstanceMilestonesQuery, List<ProjectInstanceMilestoneDto>>
{
    /// <summary>
    /// Processes the detailed task timeline retrieval query, applying optimized non-tracking projections.
    /// </summary>
    /// <param name="request">The query container holding the primary lookup identifier of the parent project instance workspace.</param>
    /// <param name="cancellationToken">Propagates notification that operational execution threads should be canceled.</param>
    /// <returns>A flat sequence list matching all local execution milestone nodes configured in the target workspace graph.</returns>
    public async Task<List<ProjectInstanceMilestoneDto>> Handle(GetProjectInstanceMilestonesQuery request, CancellationToken cancellationToken)
    {
        // Query base: Deactivate entity object change-tracking to maximize read-side processing performance.
        // Because LocalMilestone is not a top-level DbSet, we target the aggregate root and flatten via SelectMany.
        return await context.ProjectInstances
            .AsNoTracking()
            .Where(pi => pi.Id == request.ProjectInstanceId)
            .SelectMany(pi => pi.LocalMilestones)
            .Select(m => new ProjectInstanceMilestoneDto
            {
                Id = m.Id,
                ProjectInstanceId = m.ProjectInstanceId,
                TitleSnapshot = m.TitleSnapshot,
                DescriptionSnapshot = m.DescriptionSnapshot,
                ExpectedEffortInHours = m.ExpectedEffortInHours,
                RequiredDeliverableType = m.RequiredDeliverableType,
                Status = m.Status,
                ScheduledStartDate = m.ScheduledStartDate,
                ScheduledEndDate = m.ScheduledEndDate,
                SubmissionPayload = m.SubmissionPayload,
                SubmittedAt = m.SubmittedAt,
                Grade = m.Grade,
                EvaluationFeedback = m.EvaluationFeedback,
                GradedAt = m.GradedAt,

                // Project out the directed runtime milestone scheduling dependency edges
                InboundDependencies = m.InboundDependencies.Select(d => new LocalMilestoneDependencyDto(
                    d.PredecessorId,
                    d.SuccessorId,
                    d.Type
                )).ToList()
            })
            .ToListAsync(cancellationToken);
    }
}
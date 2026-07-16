using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectInstances.Queries.GetProjectInstanceMilestones;

/// <summary>
/// Handles the execution of the <see cref="GetProjectInstanceMilestonesQuery"/> request.
/// Leverages optimized aggregate-root graph loading and safe in-memory projections to respect encapsulation patterns.
/// </summary>
public class GetProjectInstanceMilestonesQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetProjectInstanceMilestonesQuery, List<ProjectInstanceMilestoneDto>>
{
    /// <summary>
    /// Processes the detailed task timeline retrieval query, loading the aggregate graph safely before mapping to DTO schemas.
    /// </summary>
    /// <param name="request">The query container holding the primary lookup identifier of the parent project instance workspace.</param>
    /// <param name="cancellationToken">Propagates notification that operational execution threads should be canceled.</param>
    /// <returns>A hierarchical structured checklist matching all local execution milestone nodes and their inner tasks configured in the target workspace graph.</returns>
    /// <exception cref="UnauthorizedAccessException">Uniformly thrown if session authentication fails, the resource is missing, or tenancy validation fails.</exception>
    public async Task<List<ProjectInstanceMilestoneDto>> Handle(GetProjectInstanceMilestonesQuery request, CancellationToken cancellationToken)
    {
        // Enforce active security session validation early before executing database logic
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to query project milestone timelines.");
        }

        // Fetch the tenancy boundary matrix from the parent aggregate root to evaluate access authority
        var projectTenancy = await context.ProjectInstances
            .AsNoTracking()
            .Where(pi => pi.Id == request.ProjectInstanceId)
            .Select(pi => new { pi.StudentId, pi.SupervisorId, pi.ProviderId })
            .FirstOrDefaultAsync(cancellationToken);

        // Validate aggregate presence and contextual user tenancy boundaries uniformly.
        // Access is strictly restricted to the participating student, supervisor, or provider.
        if (projectTenancy == null || (projectTenancy.StudentId != currentUserService.UserId &&
                                       projectTenancy.SupervisorId != currentUserService.UserId &&
                                       projectTenancy.ProviderId != currentUserService.UserId))
        {
            throw new UnauthorizedAccessException("Access Denied: The requested project workspace was not found, or you do not possess read authorization permissions.");
        }

        // Enter via the Aggregate Root to strictly respect domain boundaries.
        // Eager load child tracking entities using their EF Core configuration field backing maps.
        var projectInstance = await context.ProjectInstances
            .AsNoTracking()
            .Include(pi => pi.LocalMilestones)
                .ThenInclude(m => m.LocalTasks)
            .Include(pi => pi.LocalMilestones)
                .ThenInclude(m => m.InboundDependencies)
            .FirstOrDefaultAsync(pi => pi.Id == request.ProjectInstanceId, cancellationToken);

        if (projectInstance == null)
        {
            return [];
        }

        // Execute projection safely in-memory to elegantly handle .AsReadOnly() encapsulation barriers
        return projectInstance.LocalMilestones.Select(m => new ProjectInstanceMilestoneDto
        {
            Id = m.Id,
            ProjectInstanceId = m.ProjectInstanceId,
            TitleSnapshot = m.TitleSnapshot,
            DescriptionSnapshot = m.DescriptionSnapshot,
            ExpectedEffortInHours = m.ExpectedEffortInHours,
            Status = m.Status,
            ScheduledStartDate = m.ScheduledStartDate,
            ScheduledEndDate = m.ScheduledEndDate,
            WbsWeight = m.WbsWeight,
            GradingWeight = m.GradingWeight,
            IsWbsBalanced = m.IsWbsBalanced,

            // Project out the directed runtime milestone scheduling dependency edges
            InboundDependencies = m.InboundDependencies.Select(d => new LocalMilestoneDependencyDto(
                d.PredecessorId,
                d.SuccessorId,
                d.Type
            )).ToList(),

            // Deeply project the hierarchical child collection of nested tasks
            Tasks = m.LocalTasks.Select(t => new LocalTaskDto(
                t.Id,
                t.TitleSnapshot,
                t.DescriptionSnapshot,
                t.Weight,
                t.RequiredDeliverableType,
                t.Status,
                t.SubmissionPayload,
                t.SubmittedAt,
                t.Grade,
                t.EvaluationFeedback,
                t.GradedAt
            )).ToList()
        }).ToList();
    }
}
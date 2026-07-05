using MediatR;
using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.ProjectInstances.Queries.GetProjectInstanceMilestones;

/// <summary>
/// CQRS Query to retrieve the live collection grid matrix of localized milestone nodes 
/// and execution dependencies bound within a targeted project workspace.
/// </summary>
public record GetProjectInstanceMilestonesQuery : IRequest<List<ProjectInstanceMilestoneDto>>
{
    /// <summary>
    /// Gets the unique lookup tracking identifier of the parent ProjectInstance aggregate workspace channel root.
    /// </summary>
    public Guid ProjectInstanceId { get; init; }
}
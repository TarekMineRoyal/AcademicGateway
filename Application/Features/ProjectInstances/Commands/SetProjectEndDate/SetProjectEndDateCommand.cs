using MediatR;
using System;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.SetProjectEndDate;

/// <summary>
/// CQRS Command to adjust or extend the official operational completion deadline for a running project workspace.
/// </summary>
public record SetProjectEndDateCommand : IRequest<Unit>
{
    /// <summary>
    /// Gets the unique tracking identifier of the target project instance workspace.
    /// </summary>
    public Guid ProjectInstanceId { get; init; }

    /// <summary>
    /// Gets the newly proposed calendar date and time for the project workspace closure boundary.
    /// </summary>
    public DateTime NewEndDate { get; init; }
}
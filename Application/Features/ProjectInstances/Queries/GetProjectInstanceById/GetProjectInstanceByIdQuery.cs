using MediatR;
using System;

namespace AcademicGateway.Application.Features.ProjectInstances.Queries.GetProjectInstanceById;

/// <summary>
/// CQRS Query to retrieve the complete running execution state, participant configurations, 
/// and snapshotted parameters of a specific live project workspace channel.
/// </summary>
public record GetProjectInstanceByIdQuery : IRequest<ProjectInstanceDetailDto?>
{
    /// <summary>
    /// Gets the unique lookup tracking identifier of the targeted parent ProjectInstance aggregate root.
    /// </summary>
    public Guid Id { get; init; }
}
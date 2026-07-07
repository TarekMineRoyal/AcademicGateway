using MediatR;
using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.ProjectInstances.Queries.GetProjectsByActor;

/// <summary>
/// CQRS query request contract targeting running project snapshot workspaces.
/// Requests a unified collection of projects to populate dashboards dynamically based on the actor's identity and operational ecosystem role.
/// </summary>
public class GetProjectsByActorQuery : IRequest<IReadOnlyCollection<ActorProjectDto>>
{
    /// <summary>
    /// Gets or sets the unique tracking identifier code targeting the core ecosystem actor.
    /// </summary>
    public Guid ActorId { get; set; }

    /// <summary>
    /// Gets or sets the system authorization string role evaluating the contextual data pipeline filters (e.g., "Student", "Professor", "Provider").
    /// </summary>
    public string Role { get; set; } = string.Empty;
}
using MediatR;
using System;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.DeleteMilestoneDependency;

/// <summary>
/// CQRS Command object carrying the identifiers required to permanently sever an existing 
/// timeline dependency restriction constraint link separating two milestones within a template graph.
/// </summary>
public record DeleteMilestoneDependencyCommand : IRequest<Unit>
{
    /// <summary>
    /// Gets the unique tracking identifier of the parent ProjectTemplate aggregate root owning both milestones.
    /// </summary>
    public Guid ProjectTemplateId { get; init; }

    /// <summary>
    /// Gets the tracking identifier of the successor milestone node carrying the inbound restriction edge.
    /// </summary>
    public Guid SuccessorId { get; init; }

    /// <summary>
    /// Gets the tracking identifier of the predecessor milestone node representing the prerequisite boundary.
    /// </summary>
    public Guid PredecessorId { get; init; }
}
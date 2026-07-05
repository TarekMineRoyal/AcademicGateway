using AcademicGateway.Domain.Common.Enums;
using MediatR;
using System;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.AddMilestoneDependency;

/// <summary>
/// CQRS Command to map a directed dependency constraint edge between two existing template milestone nodes.
/// Triggers depth-first search graph processing loops to detect and mitigate circular reference traps.
/// </summary>
public record AddMilestoneDependencyCommand : IRequest<Unit>
{
    /// <summary>
    /// Gets the unique tracking identifier of the parent ProjectTemplate aggregate root owning both milestones.
    /// </summary>
    public Guid ProjectTemplateId { get; init; }

    /// <summary>
    /// Gets the tracking identifier of the successor milestone node (the item dependent on the predecessor).
    /// </summary>
    public Guid SuccessorId { get; init; }

    /// <summary>
    /// Gets the tracking identifier of the predecessor milestone node (the item that must be executed first).
    /// </summary>
    public Guid PredecessorId { get; init; }

    /// <summary>
    /// Gets the behavioural constraint type applied to this structural edge (e.g., FinishToStart, StartToStart).
    /// </summary>
    public DependencyType Type { get; init; }
}
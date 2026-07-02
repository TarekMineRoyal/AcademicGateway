using MediatR;
using System;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.StartProject;

/// <summary>
/// CQRS Command for a student to initialize a live project workspace from an approved template blueprint.
/// Spawns a new ProjectInstance aggregate root copy capturing a historical snapshot of the blueprint.
/// </summary>
public record StartProjectCommand : IRequest<Guid>
{
    /// <summary>
    /// Gets the unique identifier of the source project template blueprint to clone.
    /// </summary>
    public Guid TemplateId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the student initiating the project workspace.
    /// This is typically resolved from the authenticated student user context.
    /// </summary>
    public Guid StudentId { get; init; }

    /// <summary>
    /// Gets the optional identifier of an academic supervisor chosen at workspace startup.
    /// If provided, the instance initializes in a paused AwaitingSupervision track.
    /// </summary>
    public Guid? RequestedProfessorId { get; init; }
}
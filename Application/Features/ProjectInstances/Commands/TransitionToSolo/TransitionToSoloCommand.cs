using MediatR;
using System;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.TransitionToSolo;

/// <summary>
/// CQRS Command for a student to pull a paused project workspace out of academic matching limbo
/// and transition it into an active, independent solo execution track if the professor didn't respond.
/// </summary>
public record TransitionToSoloCommand : IRequest<Unit>
{
    /// <summary>
    /// Gets the unique tracking identifier of the target project instance workspace to pivot.
    /// </summary>
    public Guid ProjectInstanceId { get; init; }
}
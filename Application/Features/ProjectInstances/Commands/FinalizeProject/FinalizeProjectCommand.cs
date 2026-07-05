using MediatR;
using System;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.FinalizeProject;

/// <summary>
/// CQRS Command invoked to calculate, aggregate, and lock in the final overall grade score 
/// for a concluded project workspace instance as a whole.
/// Accommodates optional professor tracks by supporting execution by either the supervisor or the owner student.
/// </summary>
public record FinalizeProjectCommand : IRequest<Unit>
{
    /// <summary>
    /// Gets the unique tracking identifier of the parent ProjectInstance aggregate root workspace.
    /// </summary>
    public Guid ProjectInstanceId { get; init; }

    /// <summary>
    /// Gets the tracking account identifier of the user executing this final certification.
    /// Can represent either the assigned faculty supervisor or the owner student running an unsupervised track.
    /// </summary>
    public Guid ExecutingUserId { get; init; }
}
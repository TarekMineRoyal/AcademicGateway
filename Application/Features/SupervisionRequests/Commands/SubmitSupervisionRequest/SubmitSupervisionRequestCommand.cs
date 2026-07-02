using MediatR;
using System;

namespace AcademicGateway.Application.Features.SupervisionRequests.Commands.SubmitSupervisionRequest;

/// <summary>
/// CQRS Command to submit a new academic matchmaking invitation to a professor.
/// This alters the state of the supervision request log and ties into the parent project instance lifecycle rules.
/// </summary>
public record SubmitSupervisionRequestCommand : IRequest<Guid>
{
    /// <summary>
    /// Gets the unique tracking identifier of the parent project instance workspace.
    /// </summary>
    public Guid ProjectInstanceId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the targeted academic faculty member (professor).
    /// </summary>
    public Guid ProfessorId { get; init; }

    /// <summary>
    /// Gets the custom motivation pitch statement, goals, or proposal text composed by the student.
    /// </summary>
    public string PitchText { get; init; } = string.Empty;
}
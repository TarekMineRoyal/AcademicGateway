using MediatR;
using System;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.EvaluateTask;

/// <summary>
/// CQRS Command invoked by an academic supervisor to grade and provide feedback for a submitted nested task.
/// Triggers internal domain execution checks against the project's chosen grading strategy policy.
/// </summary>
public record EvaluateTaskCommand : IRequest<Unit>
{
    /// <summary>
    /// Gets the unique tracking identifier of the parent ProjectInstance aggregate root workspace.
    /// </summary>
    public Guid ProjectInstanceId { get; init; }

    /// <summary>
    /// Gets the unique tracking identifier of the target LocalMilestone container.
    /// </summary>
    public Guid LocalMilestoneId { get; init; }

    /// <summary>
    /// Gets the unique tracking identifier of the target child LocalTask being evaluated.
    /// </summary>
    public Guid LocalTaskId { get; init; }

    /// <summary>
    /// Gets the numerical score awarded by the grading faculty mentor.
    /// Must conform mathematically to the boundaries established by the active strategy pattern.
    /// </summary>
    public decimal Grade { get; init; }

    /// <summary>
    /// Gets the optional qualitative feedback, corrections, or assessment critique notes written by the mentor.
    /// </summary>
    public string? Feedback { get; init; }

    /// <summary>
    /// Gets the tracking account identifier of the professor submitting this evaluation pass.
    /// Used by the aggregate root to enforce supervisor identity verification boundaries.
    /// </summary>
    public Guid ExecutingProfessorId { get; init; }
}
using MediatR;
using System;

namespace AcademicGateway.Application.Features.ProjectInstances.Commands.SubmitTaskDeliverable;

/// <summary>
/// CQRS Command invoked by a student to upload or push a work deliverable for a specific nested task.
/// Triggers format validation routines matching the task's required polymorphic deliverable type configuration.
/// </summary>
public record SubmitTaskDeliverableCommand : IRequest<Unit>
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
    /// Gets the unique tracking identifier of the target child LocalTask receiving the submission.
    /// </summary>
    public Guid LocalTaskId { get; init; }

    /// <summary>
    /// Gets the raw polymorphic payload string containing the student's submission 
    /// (e.g., a URL link destination, a text summary, or a cloud file tracking hash).
    /// </summary>
    public string SubmissionPayload { get; init; } = string.Empty;
}
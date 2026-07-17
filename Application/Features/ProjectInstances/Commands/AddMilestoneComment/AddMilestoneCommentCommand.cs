using MediatR;
using System;

namespace Application.Features.ProjectInstances.Commands.AddMilestoneComment;

/// <summary>
/// CQRS Command invoked by any authorized participant (Student, Professor, or Tech Support Account) 
/// to append a collaboration comment onto a specific milestone execution lane.
/// </summary>
public record AddMilestoneCommentCommand : IRequest<Guid>
{
    /// <summary>
    /// Gets the unique tracking identifier of the parent ProjectInstance aggregate root workspace.
    /// </summary>
    public Guid ProjectInstanceId { get; init; }

    /// <summary>
    /// Gets the unique tracking identifier of the target LocalMilestone node receiving the comment.
    /// </summary>
    public Guid LocalMilestoneId { get; init; }

    /// <summary>
    /// Gets the unique account identifier code linking back to the platform user posting the entry.
    /// </summary>
    public Guid AuthorId { get; init; }

    /// <summary>
    /// Gets the functional role classification descriptor snapshot of the poster 
    /// (e.g., "Student", "Faculty Supervisor", "Corporate Technical Support").
    /// </summary>
    public string AuthorIdentitySnapshot { get; init; } = string.Empty;

    /// <summary>
    /// Gets the raw text message copy containing details, questions, or clarification notes.
    /// </summary>
    public string Content { get; init; } = string.Empty;
}
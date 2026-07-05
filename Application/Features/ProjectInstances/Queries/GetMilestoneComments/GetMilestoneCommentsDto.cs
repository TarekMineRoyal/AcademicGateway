using System;

namespace AcademicGateway.Application.Features.ProjectInstances.Queries.GetMilestoneComments;

/// <summary>
/// Data transfer object carrying the historical message data, author classification profile,
/// and commit timestamp for a single collaboration comment entry logged inside a milestone workspace leg.
/// </summary>
public record MilestoneCommentDto
{
    /// <summary>
    /// Gets the unique tracking identifier for this specific comment record.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the unique tracking identifier of the local milestone node this comment belongs to.
    /// </summary>
    public Guid LocalMilestoneId { get; init; }

    /// <summary>
    /// Gets the unique tracking account identifier of the user who authored the discussion entry.
    /// </summary>
    public Guid AuthorId { get; init; }

    /// <summary>
    /// Gets the display name or role classification description of the author captured at the moment of posting 
    /// (e.g., "Student", "Faculty Supervisor", "Corporate Technical Support").
    /// </summary>
    public string AuthorIdentitySnapshot { get; init; } = string.Empty;

    /// <summary>
    /// Gets the descriptive discussion body text containing the message content payload.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Gets the synchronized system timestamp marking exactly when this message record was committed.
    /// </summary>
    public DateTime CreatedAt { get; init; }
}
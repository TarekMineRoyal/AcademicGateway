using System;
using AcademicGateway.Domain.Common;

namespace AcademicGateway.Domain.ProjectInstances;

/// <summary>
/// Represents an immutable, timestamped text commentary entry logged inside a specific milestone execution channel.
/// Enables cross-role collaboration between Students, Faculty Supervisors, and Corporate Tech Support mentors.
/// </summary>
public class MilestoneComment : BaseEntity
{
    /// <summary>
    /// Gets the unique tracking identifier for this specific comment row.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the foreign tracking link mapping back to the target local milestone node.
    /// </summary>
    public Guid LocalMilestoneId { get; private set; }

    /// <summary>
    /// Gets the unique tracking account identifier of the user who authored the entry.
    /// </summary>
    public Guid AuthorId { get; private set; }

    /// <summary>
    /// Gets the display name or role classification description of the author at the moment of posting 
    /// (e.g., "Student", "Faculty Supervisor", "Corporate Technical Support").
    /// </summary>
    public string AuthorIdentitySnapshot { get; private set; }

    /// <summary>
    /// Gets the descriptive discussion body text containing the message content payload.
    /// </summary>
    public string Content { get; private set; }

    /// <summary>
    /// Gets the synchronized system timestamp marking exactly when this message record was committed.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Parameterless constructor required by EF Core for data materialization loops.
    /// </summary>
    private MilestoneComment()
    {
        AuthorIdentitySnapshot = null!;
        Content = null!;
    }

    /// <summary>
    /// Initializes a new production instance of a milestone collaboration comment entry.
    /// </summary>
    /// <param name="localMilestoneId">The unique tracking token of the target local milestone context.</param>
    /// <param name="authorId">The unique account identifier code linking back to the posting platform user.</param>
    /// <param name="authorIdentitySnapshot">The string description defining the functional role authority of the poster.</param>
    /// <param name="content">The raw message textual copy containing details, questions, or clarification instructions.</param>
    /// <param name="createdAt">The deterministic synchronized execution timestamp variable.</param>
    /// <exception cref="ArgumentException">Thrown if empty text constraints or structural parameter limits are violated.</exception>
    public MilestoneComment(
        Guid localMilestoneId,
        Guid authorId,
        string authorIdentitySnapshot,
        string content,
        DateTime createdAt)
    {
        if (localMilestoneId == Guid.Empty)
        {
            throw new ArgumentException("The target local milestone tracking identifier cannot be empty.", nameof(localMilestoneId));
        }

        if (authorId == Guid.Empty)
        {
            throw new ArgumentException("The author tracking identifier account token cannot be empty.", nameof(authorId));
        }

        if (string.IsNullOrWhiteSpace(authorIdentitySnapshot))
        {
            throw new ArgumentException("The author identity snapshot role descriptor copy cannot be empty or whitespace.", nameof(authorIdentitySnapshot));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("The discussion comment message copy content cannot be empty or whitespace.", nameof(content));
        }

        if (content.Trim().Length > 2000)
        {
            throw new ArgumentException("Discussion comment copy contents cannot exceed the maximum length boundary of 2000 characters.", nameof(content));
        }

        Id = Guid.NewGuid();
        LocalMilestoneId = localMilestoneId;
        AuthorId = authorId;
        AuthorIdentitySnapshot = authorIdentitySnapshot.Trim();
        Content = content.Trim();
        CreatedAt = createdAt;
    }
}
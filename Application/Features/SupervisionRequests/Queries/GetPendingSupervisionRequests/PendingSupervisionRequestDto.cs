using System;

namespace AcademicGateway.Application.Features.SupervisionRequests.Queries.GetPendingSupervisionRequests;

/// <summary>
/// Data transfer object representing an outstanding, pending academic supervision request.
/// Exposes a flattened read-only payload required by a faculty member to evaluate an incoming matchmaking invitation.
/// </summary>
public class PendingSupervisionRequestDto
{
    /// <summary>
    /// Gets or sets the unique tracking identifier for the supervision request.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the tracking identifier for the parent live project workspace aggregate.
    /// </summary>
    public Guid ProjectInstanceId { get; set; }

    /// <summary>
    /// Gets or sets the snapshotted headline title of the project instance requesting mentorship.
    /// </summary>
    public string ProjectTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the snapshotted core requirements overview text of the project instance.
    /// </summary>
    public string ProjectDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the motivational text, goals, or proposal pitch written by the student.
    /// </summary>
    public string PitchText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the precise timestamp recording when this invitation was issued.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
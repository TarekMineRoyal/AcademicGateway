using AcademicGateway.Domain.ProjectInstances.Enums;
using System;

namespace AcademicGateway.Application.Features.ProjectInstances.Queries.GetProjectsByActor;

/// <summary>
/// Data transfer object representing a flattened overview summary of a running project workspace channel.
/// Serves as a unified read-only row contract designed to populate multi-role ecosystem dashboards.
/// </summary>
public class ActorProjectDto
{
    /// <summary>
    /// Gets or sets the unique tracking identifier for the live project instance workspace.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier code mapping back to the owner student initiator.
    /// </summary>
    public Guid StudentId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier code tracking the co-managing academic faculty mentor, if any.
    /// </summary>
    public Guid? SupervisorId { get; set; }

    /// <summary>
    /// Gets or sets the identifier code tracking the corporate provider who published the blueprint template.
    /// </summary>
    public Guid ProviderId { get; set; }

    /// <summary>
    /// Gets or sets the snapshotted copy of the project's headline title text.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the snapshotted copy of the project's core requirements overview text.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current processing lifecycle state within active platform matching or execution paths.
    /// </summary>
    public ProjectInstanceStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the point-in-time synchronized timestamp recording when this running project workspace was initializing.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the strict administrative deadline capping active work execution, if assigned.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Gets or sets the final evaluation numerical score awarded to the aggregate project workspace once concluded.
    /// </summary>
    public decimal? OverallGrade { get; set; }
}
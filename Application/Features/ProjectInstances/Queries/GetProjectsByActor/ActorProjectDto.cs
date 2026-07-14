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

    // =========================================================================
    // EXTENDED PROPERTIES FOR REDESIGNED STUDENT DASHBOARD
    // =========================================================================

    /// <summary>
    /// Gets or sets the snapshot text title of the milestone currently marked in-progress, if any.
    /// </summary>
    public string? CurrentMilestoneTitle { get; set; }

    /// <summary>
    /// Gets or sets the accumulated numerical tracking progress ratio for tasks in the active milestone.
    /// </summary>
    public decimal CurrentMilestoneProgress { get; set; }

    /// <summary>
    /// Gets or sets the macro cumulative completion progress score across the entire project lifespan.
    /// </summary>
    public decimal TotalProjectProgress { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the active professor mentor supervising this active project.
    /// </summary>
    public Guid? ProfessorId { get; set; }

    /// <summary>
    /// Gets or sets the full name string snapshot of the supervising professor mentor.
    /// </summary>
    public string? ProfessorName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this active project is operating without assigned faculty supervision.
    /// </summary>
    public bool IsSoloMode { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier mapping to the targeted professor for an unapproved, pending supervision request.
    /// </summary>
    public Guid? RequestedProfessorId { get; set; }

    /// <summary>
    /// Gets or sets the full name string copy of the professor tied to a pending matchmaking supervision request.
    /// </summary>
    public string? RequestedProfessorName { get; set; }

    /// <summary>
    /// Gets or sets the explicit corporate company name published by the entity providing the template blueprint.
    /// </summary>
    public string ProviderCompanyName { get; set; } = string.Empty;
}
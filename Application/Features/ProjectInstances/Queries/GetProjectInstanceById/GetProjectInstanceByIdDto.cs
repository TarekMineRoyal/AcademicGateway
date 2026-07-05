using AcademicGateway.Domain.ProjectInstances.Enums;
using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.ProjectInstances.Queries.GetProjectInstanceById;

/// <summary>
/// Data transfer object carrying the complete running runtime state, participant bindings, 
/// and administrative parameters of an active project instance workspace channel.
/// </summary>
public record ProjectInstanceDetailDto
{
    /// <summary>
    /// Gets the unique tracking identifier for the live project workspace.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the identifier code mapping back to the owner student account.
    /// </summary>
    public Guid StudentId { get; init; }

    /// <summary>
    /// Gets the full name of the owner student profile.
    /// </summary>
    public string StudentName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the identifier code mapping to the assigned faculty mentor, if any supervisor has signed on.
    /// </summary>
    public Guid? SupervisorId { get; init; }

    /// <summary>
    /// Gets the full name of the co-managing academic faculty mentor, if assigned.
    /// </summary>
    public string? SupervisorName { get; init; }

    /// <summary>
    /// Gets the identifier of the source template blueprint this instance was spawned from.
    /// </summary>
    public Guid TemplateId { get; init; }

    /// <summary>
    /// Gets the identifier of the creating corporate provider organization.
    /// </summary>
    public Guid ProviderId { get; init; }

    /// <summary>
    /// Gets the historical snapshot copy of the project's headline title.
    /// </summary>
    public string TitleSnapshot { get; init; } = string.Empty;

    /// <summary>
    /// Gets the historical snapshot copy of the project's core requirements overview text.
    /// </summary>
    public string DescriptionSnapshot { get; init; } = string.Empty;

    /// <summary>
    /// Gets the current state within the active platform matching and execution lifecycle.
    /// </summary>
    public ProjectInstanceStatus Status { get; init; }

    /// <summary>
    /// Gets the point-in-time timestamp indicating when this live workspace was initialized.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the strict administrative deadline capping active work execution, set by the supervisor.
    /// </summary>
    public DateTime? EndDate { get; init; }

    /// <summary>
    /// Gets the final numerical score value awarded to the project aggregate as a whole upon completion.
    /// </summary>
    public decimal? OverallGrade { get; init; }

    /// <summary>
    /// Gets the timestamp tracking when the project aggregate's final evaluation was certified.
    /// </summary>
    public DateTime? ProjectGradedAt { get; init; }

    /// <summary>
    /// Gets the list of technical skills snapshotted for this specific running project instance workspace.
    /// </summary>
    public List<InstanceSkillDto> SnapshotSkills { get; init; } = [];
}

/// <summary>
/// Sub-DTO tracking technical capability mappings for an active project workspace instance.
/// </summary>
public record InstanceSkillDto(Guid SkillId, string Name);
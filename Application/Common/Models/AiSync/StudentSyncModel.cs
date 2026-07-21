using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Common.Models.AiSync;

/// <summary>
/// Fat DTO payload sent to POST /api/v1/sync/student for AI vector matchmaking index synchronization.
/// </summary>
public class StudentSyncModel
{
    /// <summary>
    /// Gets or sets the nested student entity representation.
    /// </summary>
    public StudentPayload Student { get; set; } = new();

    /// <summary>
    /// Gets or sets the resolved text label for the primary academic major.
    /// </summary>
    public string? MajorName { get; set; }

    /// <summary>
    /// Gets or sets the resolved text labels for minor academic specialty concentrations.
    /// </summary>
    public List<string> SpecialtyNames { get; set; } = new();

    /// <summary>
    /// Gets or sets the resolved text labels for technical skills.
    /// </summary>
    public List<string> SkillNames { get; set; } = new();
}

/// <summary>
/// Nested student domain data within the sync contract.
/// </summary>
public class StudentPayload
{
    /// <summary>
    /// Gets or sets the student unique identifier (maps 1:1 to Identity User ID).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the legal full display name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary major identifier.
    /// </summary>
    public Guid? MajorId { get; set; }

    /// <summary>
    /// Gets or sets the list of minor specialty identifiers.
    /// </summary>
    public List<Guid> SpecialtyIds { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of technical skill identifiers.
    /// </summary>
    public List<Guid> SkillIds { get; set; } = new();

    /// <summary>
    /// Gets or sets the biography / summary text.
    /// </summary>
    public string? AboutMe { get; set; }
}
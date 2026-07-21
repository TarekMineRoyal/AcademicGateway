using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Common.Models.AiSync;

/// <summary>
/// Fat DTO payload sent to POST /api/v1/sync/professor for AI vector matchmaking index synchronization.
/// </summary>
public class ProfessorSyncModel
{
    /// <summary>
    /// Gets or sets the nested professor entity representation.
    /// </summary>
    public ProfessorPayload Professor { get; set; } = new();

    /// <summary>
    /// Gets or sets the resolved text labels for research interest areas.
    /// </summary>
    public List<string> InterestAreas { get; set; } = new();
}

/// <summary>
/// Nested professor domain data within the sync contract.
/// </summary>
public class ProfessorPayload
{
    /// <summary>
    /// Gets or sets the professor unique identifier (maps 1:1 to Identity User ID).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the legal full display name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target academic department division text.
    /// </summary>
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the professional rank title status.
    /// </summary>
    public string Rank { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the professor is accepting new project supervisions.
    /// </summary>
    public bool IsAcceptingProjects { get; set; }

    /// <summary>
    /// Gets or sets the list of associated research interest identifiers.
    /// </summary>
    public List<Guid> ResearchInterestIds { get; set; } = new();

    /// <summary>
    /// Gets or sets the biographical summary text.
    /// </summary>
    public string? AboutMe { get; set; }
}
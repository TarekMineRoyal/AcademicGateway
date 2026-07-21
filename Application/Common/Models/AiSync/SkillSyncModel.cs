using System;

namespace AcademicGateway.Application.Common.Models.AiSync;

/// <summary>
/// DTO payload sent to POST /api/v1/sync/skill for AI vector matchmaking index synchronization.
/// </summary>
public class SkillSyncModel
{
    /// <summary>
    /// Gets or sets the nested skill entity representation.
    /// </summary>
    public SkillPayload Skill { get; set; } = new();
}

/// <summary>
/// Nested skill domain data within the sync contract.
/// </summary>
public class SkillPayload
{
    /// <summary>
    /// Gets or sets the unique skill identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the descriptive title or name of the skill.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
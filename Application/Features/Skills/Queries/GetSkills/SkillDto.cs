using System;

namespace AcademicGateway.Application.Features.Skills.Queries.GetSkills;

/// <summary>
/// Data Transfer Object representing a technical skill or professional competency asset.
/// Immutably transfers global capability lookup records out to presentation or registration layers.
/// </summary>
public record SkillDto
{
    /// <summary>
    /// Gets the unique entity identifier assigned to this specific skill lookup row.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the descriptive name or title of the competency area (e.g., "C# Programming", "Data Analysis").
    /// </summary>
    public string Name { get; init; } = string.Empty;
}
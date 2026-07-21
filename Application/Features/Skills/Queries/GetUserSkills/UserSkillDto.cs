using System;

namespace AcademicGateway.Application.Features.Skills.Queries.GetUserSkills;

/// <summary>
/// Data Transfer Object representing a user's assigned technical skill or professional competency asset[cite: 5].
/// Immutably transfers assigned profile capability records out to presentation or registration layers[cite: 5].
/// </summary>
public record UserSkillDto
{
    /// <summary>
    /// Gets the unique entity identifier assigned to this specific skill lookup row[cite: 5].
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the descriptive name or title of the competency area (e.g., "C# Programming", "Data Analysis")[cite: 5].
    /// </summary>
    public string Name { get; init; } = string.Empty;
}
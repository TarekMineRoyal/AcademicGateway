using System;
using System.Collections.Generic;
using Domain.Students;

namespace Domain.Lookups;

/// <summary>
/// Represents a technical or professional skill within the gateway (e.g., C#, Project Management, Data Analysis).
/// </summary>
public class Skill
{
    private readonly List<StudentSkill> _studentSkills = new();

    /// <summary>
    /// Gets the unique identifier for the Skill.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the unique, descriptive name of the skill.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the read-only tracking collection of student profiles mapped to this skill.
    /// </summary>
    public IReadOnlyCollection<StudentSkill> StudentSkills => _studentSkills.AsReadOnly();

    /// <summary>
    /// EF Core constructor requirement. Prevents bypass of business validations during standard initialization.
    /// </summary>
    private Skill()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Skill"/> class with required business validations.
    /// </summary>
    /// <param name="name">The name of the skill.</param>
    /// <exception cref="ArgumentException">Thrown when the name is null, empty, or whitespace.</exception>
    public Skill(string name)
    {
        UpdateName(name);
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Updates the descriptive name of the skill, verifying business rule boundaries.
    /// </summary>
    /// <param name="newName">The new name for the skill.</param>
    /// <exception cref="ArgumentException">Thrown if the provided name is invalid.</exception>
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("Skill name cannot be empty or whitespace.", nameof(newName));
        }

        Name = newName.Trim();
    }
}
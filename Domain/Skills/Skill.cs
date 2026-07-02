using System;
using System.Collections.Generic;
using AcademicGateway.Domain.Common;
using AcademicGateway.Domain.Skills.Exceptions;
using AcademicGateway.Domain.Students;

namespace AcademicGateway.Domain.Skills;

/// <summary>
/// Represents a standardized technical or professional competency lookup index within the gateway 
/// (e.g., C#, Project Management, Data Analysis).
/// </summary>
public class Skill : BaseEntity
{
    private readonly List<StudentSkill> _studentSkills = new();

    /// <summary>
    /// Gets the unique surrogate tracking identification primary key for this Skill.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the unique, descriptive text title identifier of the skill capability.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a read-only encapsulated collection of student profiles mapped to this skill tracking matrix.
    /// </summary>
    public IReadOnlyCollection<StudentSkill> StudentSkills => _studentSkills.AsReadOnly();

    /// <summary>
    /// Required parameterless constructor variant for Entity Framework Core relational database hydration mappings.
    /// Prevents bypass of domain constraints during persistence hydration.
    /// </summary>
    private Skill()
    {
    }

    /// <summary>
    /// Initializes a new valid domain instance of the <see cref="Skill"/> aggregate lookup entity.
    /// </summary>
    /// <param name="name">The descriptive textual title assigned onto the professional capability.</param>
    /// <exception cref="EmptySkillNameException">Thrown when the name argument parameter verification fails format criteria bounds.</exception>
    public Skill(string name)
    {
        UpdateName(name);
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Mutates the unique descriptive name of the skill lookup entity after verifying baseline string invariants.
    /// </summary>
    /// <param name="newName">The new target title value expression string to save into the database row properties.</param>
    /// <exception cref="EmptySkillNameException">Thrown when the input evaluation discovers a null or whitespace text string argument.</exception>
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new EmptySkillNameException();
        }

        Name = newName.Trim();
    }
}
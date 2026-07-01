using System;
using Domain.Skills;

namespace Domain.Students;

/// <summary>
/// Represents the explicit many-to-many join entity linking a Student profile to a specific technical or professional Skill.
/// </summary>
public class StudentSkill
{
    /// <summary>
    /// Gets the foreign key identifier for the associated student profile (corresponds to the unique Identity User ID).
    /// </summary>
    public Guid StudentId { get; private set; }

    /// <summary>
    /// Gets the navigation property for the associated student profile.
    /// </summary>
    public Student Student { get; private set; } = null!;

    /// <summary>
    /// Gets the foreign key identifier for the associated skill.
    /// </summary>
    public Guid SkillId { get; private set; }

    /// <summary>
    /// Gets the navigation property for the associated skill.
    /// </summary>
    public Skill Skill { get; private set; } = null!;

    /// <summary>
    /// EF Core constructor requirement. Prevents bypass of standard initialization during database materialization.
    /// </summary>
    private StudentSkill()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StudentSkill"/> join entity with required tracking identifiers.
    /// </summary>
    /// <param name="studentId">The unique Identity string identifier of the target student profile.</param>
    /// <param name="skillId">The unique identifier of the target skill.</param>
    /// <exception cref="ArgumentException">Thrown when studentId is null/whitespace or skillId is an empty Guid.</exception>
    public StudentSkill(Guid studentId, Guid skillId)
    {
        if (studentId == Guid.Empty) throw new ArgumentException("Student ID cannot be empty.", nameof(studentId));
        if (skillId == Guid.Empty) throw new ArgumentException("Skill ID cannot be empty.", nameof(skillId));

        StudentId = studentId;
        SkillId = skillId;
    }
}
using System;
using Domain.Skills;

namespace Domain.ProjectTemplates;

/// <summary>
/// Represents the explicit many-to-many join entity linking a <see cref="ProjectTemplate"/> blueprint to a required academic or technical <see cref="Skill"/>[cite: 1].
/// </summary>
public class ProjectTemplateSkill
{
    /// <summary>
    /// Gets the foreign key identifier for the associated project template[cite: 1].
    /// </summary>
    public Guid ProjectTemplateId { get; private set; }

    /// <summary>
    /// Gets the navigation property for the associated project template[cite: 1].
    /// </summary>
    public ProjectTemplate ProjectTemplate { get; private set; } = null!;

    /// <summary>
    /// Gets the foreign key identifier for the associated skill requirements metric[cite: 1].
    /// </summary>
    public Guid SkillId { get; private set; }

    /// <summary>
    /// Gets the navigation property for the associated skill requirements metric[cite: 1].
    /// </summary>
    public Skill Skill { get; private set; } = null!;

    /// <summary>
    /// EF Core constructor requirement. Prevents bypass of standard initialization constraints during database hydration.
    /// </summary>
    private ProjectTemplateSkill()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectTemplateSkill"/> join entity with required tracking keys.
    /// </summary>
    /// <param name="projectTemplateId">The unique tracking identifier of the target project template.</param>
    /// <param name="skillId">The unique tracking identifier of the target skill.</param>
    /// <exception cref="ArgumentException">Thrown when either tracking identifier is an empty Guid.</exception>
    public ProjectTemplateSkill(Guid projectTemplateId, Guid skillId)
    {
        if (projectTemplateId == Guid.Empty)
        {
            throw new ArgumentException("Project template ID cannot be empty.", nameof(projectTemplateId));
        }

        if (skillId == Guid.Empty)
        {
            throw new ArgumentException("Skill ID cannot be empty.", nameof(skillId));
        }

        ProjectTemplateId = projectTemplateId;
        SkillId = skillId;
    }
}
using System;
using AcademicGateway.Domain.ProjectTemplates.Exceptions;
using AcademicGateway.Domain.Skills;

namespace AcademicGateway.Domain.ProjectTemplates;

/// <summary>
/// Represents the explicit many-to-many join entity linking a <see cref="ProjectTemplate"/> blueprint 
/// to a required academic or technical competency <see cref="Skill"/>.
/// Acts strictly as an immutable structural mapping entity inside the ProjectTemplates aggregate boundary workspace.
/// </summary>
public class ProjectTemplateSkill
{
    /// <summary>
    /// Gets the unique foreign key identifier for the associated project template.
    /// </summary>
    public Guid ProjectTemplateId { get; private set; }

    /// <summary>
    /// Gets the formal domain navigation relationship property pointing back to the associated parent project template aggregate root.
    /// </summary>
    public ProjectTemplate ProjectTemplate { get; private set; } = null!;

    /// <summary>
    /// Gets the unique foreign key identifier for the associated skill requirements metric.
    /// </summary>
    public Guid SkillId { get; private set; }

    /// <summary>
    /// Gets the formal domain navigation relationship property pointing to the associated skill lookup entity.
    /// </summary>
    public Skill Skill { get; private set; } = null!;

    /// <summary>
    /// Required parameterless constructor variant for Entity Framework Core relational database hydration mappings.
    /// Prevents bypass of standard initialization constraints during database hydration.
    /// </summary>
    private ProjectTemplateSkill()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectTemplateSkill"/> join entity with required tracking keys.
    /// </summary>
    /// <param name="projectTemplateId">The unique tracking identifier of the target project template.</param>
    /// <param name="skillId">The unique tracking identifier of the target skill.</param>
    /// <exception cref="InvalidTemplateDetailsException">Thrown when either tracking identifier is an empty Guid.</exception>
    public ProjectTemplateSkill(Guid projectTemplateId, Guid skillId)
    {
        if (projectTemplateId == Guid.Empty)
        {
            throw new InvalidTemplateDetailsException("Project template identification tracker context cannot be empty.");
        }

        if (skillId == Guid.Empty)
        {
            throw new InvalidTemplateDetailsException("Skill identity tracking reference context cannot be empty.");
        }

        ProjectTemplateId = projectTemplateId;
        SkillId = skillId;
    }
}
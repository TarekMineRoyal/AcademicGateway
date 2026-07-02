using System;

namespace AcademicGateway.Domain.ProjectInstances;

/// <summary>
/// Represents a immutable historical snapshot of a technical skill competency required by 
/// this specific project instance at the moment of its creation.
/// </summary>
public class ProjectInstanceSkill
{
    /// <summary>
    /// Gets the unique identifier for the parent project instance.
    /// </summary>
    public Guid ProjectInstanceId { get; private set; }

    /// <summary>
    /// Gets the unique identifier for the snapshotted technical skill.
    /// </summary>
    public Guid SkillId { get; private set; }

    /// <summary>
    /// EF Core constructor requirement for hydration.
    /// </summary>
    private ProjectInstanceSkill()
    {
    }

    /// <summary>
    /// Initializes a new point-in-time skill snapshot record.
    /// </summary>
    /// <param name="projectInstanceId">The target live project workspace identifier.</param>
    /// <param name="skillId">The target skill identifier code.</param>
    public ProjectInstanceSkill(Guid projectInstanceId, Guid skillId)
    {
        ProjectInstanceId = projectInstanceId;
        SkillId = skillId;
    }
}
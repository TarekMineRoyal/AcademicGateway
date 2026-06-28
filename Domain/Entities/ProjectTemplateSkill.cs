namespace AcademicGateway.Domain.Entities;

public class ProjectTemplateSkill
{
    public Guid ProjectTemplateId { get; private set; }
    public ProjectTemplate ProjectTemplate { get; private set; } = null!;

    public Guid SkillId { get; private set; }
    public Skill Skill { get; private set; } = null!;

    private ProjectTemplateSkill() { }

    public ProjectTemplateSkill(Guid projectTemplateId, Guid skillId)
    {
        ProjectTemplateId = projectTemplateId;
        SkillId = skillId;
    }
}
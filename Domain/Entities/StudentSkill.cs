namespace AcademicGateway.Domain.Entities;

public class StudentSkill
{
    public string StudentId { get; set; } = string.Empty;
    public Student Student { get; set; } = null!;

    public Guid SkillId { get; set; }
    public Skill Skill { get; set; } = null!;
}
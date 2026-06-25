namespace AcademicGateway.Domain.Entities;

public class Skill
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation property
    public ICollection<StudentSkill> StudentSkills { get; set; } = new List<StudentSkill>();
}
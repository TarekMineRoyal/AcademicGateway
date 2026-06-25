namespace AcademicGateway.Domain.Entities;

public class Student
{
    // Acts as PK and FK to the Identity User
    public string UserId { get; set; } = string.Empty;

    public string Major { get; set; } = string.Empty;
    public string? Specialty { get; set; }
    public int? GraduationYear { get; set; }

    // Navigation property for the many-to-many relationship
    public ICollection<StudentSkill> StudentSkills { get; set; } = new List<StudentSkill>();
}
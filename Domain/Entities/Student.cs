namespace AcademicGateway.Domain.Entities;

public class Student
{
    public string UserId { get; set; } = string.Empty;
    public int? GraduationYear { get; set; }
    public ICollection<StudentMajor> StudentMajors { get; set; } = new List<StudentMajor>();
    public ICollection<StudentSpecialty> StudentSpecialties { get; set; } = new List<StudentSpecialty>();
    public ICollection<StudentSkill> StudentSkills { get; set; } = new List<StudentSkill>();
}
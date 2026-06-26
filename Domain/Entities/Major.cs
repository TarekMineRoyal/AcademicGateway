namespace AcademicGateway.Domain.Entities;

public class Major
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation property: One major has many specialties
    public ICollection<Specialty> Specialties { get; set; } = new List<Specialty>();

    public ICollection<StudentMajor> StudentMajors { get; set; } = new List<StudentMajor>();
}
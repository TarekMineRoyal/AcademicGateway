namespace AcademicGateway.Domain.Entities;

public class Specialty
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Foreign Key linking this specialty to a specific major
    public Guid MajorId { get; set; }
    public Major Major { get; set; } = null!;

    public ICollection<StudentSpecialty> StudentSpecialties { get; set; } = new List<StudentSpecialty>();
}
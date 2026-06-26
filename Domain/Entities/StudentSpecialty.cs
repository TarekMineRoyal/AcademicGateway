namespace AcademicGateway.Domain.Entities;

public class StudentSpecialty
{
    public string StudentId { get; set; } = string.Empty;
    public Student Student { get; set; } = null!;

    public Guid SpecialtyId { get; set; }
    public Specialty Specialty { get; set; } = null!;
}
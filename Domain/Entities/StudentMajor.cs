namespace AcademicGateway.Domain.Entities;

public class StudentMajor
{
    public string StudentId { get; set; } = string.Empty;
    public Student Student { get; set; } = null!;

    public Guid MajorId { get; set; }
    public Major Major { get; set; } = null!;
}
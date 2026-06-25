namespace AcademicGateway.Domain.Entities;

public class Professor
{
    // Acts as PK and FK to the Identity User
    public string UserId { get; set; } = string.Empty;

    public string AcademicDepartment { get; set; } = string.Empty;
}
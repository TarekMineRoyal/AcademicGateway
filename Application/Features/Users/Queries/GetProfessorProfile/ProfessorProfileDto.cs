namespace AcademicGateway.Application.Features.Users.Queries.GetProfessorProfile;

public record ProfessorProfileDto
{
    public Guid UserId { get; init; }
    public string AcademicDepartment { get; init; } = string.Empty;
}
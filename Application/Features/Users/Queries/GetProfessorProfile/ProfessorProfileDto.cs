namespace AcademicGateway.Application.Features.Users.Queries.GetProfessorProfile;

public record ProfessorProfileDto
{
    public string UserId { get; init; } = string.Empty;
    public string AcademicDepartment { get; init; } = string.Empty;
}
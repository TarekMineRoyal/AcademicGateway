using MediatR;

namespace AcademicGateway.Application.Features.Users.Commands.RegisterProfessor;

// The DTO specifically tailored for Professor registration
public record RegisterProfessorCommand : IRequest<string>
{
    // Base Identity Properties
    public string Email { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;

    // Professor Specific Properties
    public string AcademicDepartment { get; init; } = string.Empty;
}
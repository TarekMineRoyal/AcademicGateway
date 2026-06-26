using MediatR;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.Users.Commands.RegisterStudent;

// The Command acts as our DTO containing all necessary registration info
public record RegisterStudentCommand : IRequest<string>
{
    public string Email { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Major { get; init; } = string.Empty;
    public string? Specialty { get; init; }
    public int? GraduationYear { get; init; }

    // List of selected skills
    public List<Guid> SkillIds { get; init; } = new();
}
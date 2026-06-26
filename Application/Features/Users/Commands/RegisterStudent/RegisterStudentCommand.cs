using MediatR;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.Users.Commands.RegisterStudent;

public record RegisterStudentCommand : IRequest<string>
{
    public string Email { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public int? GraduationYear { get; init; }

    // Replaced the strings with lists of IDs
    public List<Guid> MajorIds { get; init; } = new();
    public List<Guid> SpecialtyIds { get; init; } = new();
    public List<Guid> SkillIds { get; init; } = new();
}
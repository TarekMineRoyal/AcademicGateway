using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.Users.Queries.GetStudentProfile;

public record StudentProfileDto
{
    public Guid UserId { get; init; }
    public int? GraduationYear { get; init; }
    public List<StudentMajorDto> Majors { get; init; } = new();
    public List<StudentSpecialtyDto> Specialties { get; init; } = new();
    public List<StudentSkillDto> Skills { get; init; } = new();
}

public record StudentMajorDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}

public record StudentSpecialtyDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}

public record StudentSkillDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
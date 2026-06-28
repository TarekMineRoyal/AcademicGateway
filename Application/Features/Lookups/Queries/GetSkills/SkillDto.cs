using System;

namespace AcademicGateway.Application.Features.Lookups.Queries.GetSkills;

public record SkillDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
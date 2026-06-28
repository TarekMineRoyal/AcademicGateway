namespace AcademicGateway.Application.Features.Lookups.Queries.GetMajors;

public record MajorDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public List<SpecialtyDto> Specialties { get; init; } = new();
}

public record SpecialtyDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
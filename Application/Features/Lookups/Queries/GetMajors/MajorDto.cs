using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.Lookups.Queries.GetMajors;

/// <summary>
/// Data Transfer Object representing an academic major curriculum boundary.
/// Immutably transfers master program data out to presentation or external API layers.
/// </summary>
public record MajorDto
{
    /// <summary>
    /// Gets the global unique entity identifier for the academic major.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the unique, official programmatic name of the major (e.g., "Computer Science").
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the read-only collection of fine-grained educational sub-specialties housed under this major program.
    /// </summary>
    public IReadOnlyCollection<SpecialtyDto> Specialties { get; init; } = Array.Empty<SpecialtyDto>();
}

/// <summary>
/// Data Transfer Object representing a nested sub-specialty map belonging directly to an parent academic major.
/// </summary>
public record SpecialtyDto
{
    /// <summary>
    /// Gets the global unique entity identifier for the specific specialty stream.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the official name of the specialty focus area (e.g., "Artificial Intelligence").
    /// </summary>
    public string Name { get; init; } = string.Empty;
}
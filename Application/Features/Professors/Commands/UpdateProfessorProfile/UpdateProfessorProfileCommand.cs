using MediatR;
using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.Professors.Commands.UpdateProfessorProfile;

/// <summary>
/// CQRS Command to update an existing professor's institutional profile, faculty details, 
/// mentoring capacities, and active research interest alignments.
/// </summary>
public record UpdateProfessorProfileCommand : IRequest
{
    /// <summary>
    /// Gets the updated legal full display name tracking this faculty member's identity records.
    /// </summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the updated summary/biography text used by the vector matchmaking engine for recommendations.
    /// </summary>
    public string? AboutMe { get; init; }

    /// <summary>
    /// Gets the updated target academic department division designation text (e.g., "Computer Science").
    /// </summary>
    public string Department { get; init; } = string.Empty;

    /// <summary>
    /// Gets the updated professional instructional rank title status (e.g., "Associate Professor").
    /// </summary>
    public string Rank { get; init; } = string.Empty;

    /// <summary>
    /// Gets the updated maximum ceiling boundary number of student projects this professor can supervise simultaneously.
    /// </summary>
    public int MaxSupervisionCapacity { get; init; }

    /// <summary>
    /// Gets the complete collection of fine-grained research interest tracking identifiers chosen by this professor.
    /// Existing interest mappings not present in this collection will be cleanly removed.
    /// </summary>
    public IReadOnlyCollection<Guid> ResearchInterestIds { get; init; } = Array.Empty<Guid>();
}
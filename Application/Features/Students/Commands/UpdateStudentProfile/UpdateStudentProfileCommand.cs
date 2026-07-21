using MediatR;
using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.Students.Commands.UpdateStudentProfile;

/// <summary>
/// CQRS Command to update an existing student profile's properties, technical capabilities, and academic paths.
/// Enforces context-secure profile maintenance workflows within the Student aggregate boundary.
/// </summary>
public record UpdateStudentProfileCommand : IRequest
{
    /// <summary>
    /// Gets the updated formal legal full display name tracking this student's identity records.
    /// </summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the updated optional biography or self-description text summarizing the student.
    /// </summary>
    public string? AboutMe { get; init; }

    /// <summary>
    /// Gets the updated optional target graduation completion calendar year logged by the student.
    /// </summary>
    public int? GraduationYear { get; init; }

    /// <summary>
    /// Gets the complete collection of universal academic major tracking identifiers chosen by this student.
    /// Existing major mappings not present in this collection will be cleanly removed.
    /// </summary>
    public IReadOnlyCollection<Guid> MajorIds { get; init; } = Array.Empty<Guid>();

    /// <summary>
    /// Gets the complete collection of fine-grained sub-track specialty identifiers chosen by this student.
    /// Existing specialty mappings not present in this collection will be cleanly removed.
    /// </summary>
    public IReadOnlyCollection<Guid> SpecialtyIds { get; init; } = Array.Empty<Guid>();

    /// <summary>
    /// Gets the complete collection of technical capability or competency skill identifiers claimed by this student.
    /// Existing skill inventory rows not present in this collection will be cleanly removed.
    /// </summary>
    public IReadOnlyCollection<Guid> SkillIds { get; init; } = Array.Empty<Guid>();
}
using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.Students.Queries.GetStudentProfile;

/// <summary>
/// Data Transfer Object representing the comprehensive profile view of an academic student.
/// Exposes core profile states, declared majors, technical specialties, and claimed skill inventories.
/// </summary>
public record StudentProfileDto
{
    /// <summary>
    /// Gets the global unique entity identifier tracking this student profile, mapping 1:1 to their security credentials.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the legal full display name tracking this student.
    /// </summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the targeted graduation completion calendar year logged by the student.
    /// </summary>
    public int? GraduationYear { get; init; }

    /// <summary>
    /// Gets the read-only collection of assigned academic major programs tracking this student.
    /// </summary>
    public IReadOnlyCollection<StudentMajorDto> Majors { get; init; } = Array.Empty<StudentMajorDto>();

    /// <summary>
    /// Gets the read-only collection of fine-grained structural educational sub-specialties chosen by this student.
    /// </summary>
    public IReadOnlyCollection<StudentSpecialtyDto> Specialties { get; init; } = Array.Empty<StudentSpecialtyDto>();

    /// <summary>
    /// Gets the read-only collection of technical capability or competency skills possessed by this student.
    /// </summary>
    public IReadOnlyCollection<StudentSkillDto> Skills { get; init; } = Array.Empty<StudentSkillDto>();
}

/// <summary>
/// Data Transfer Object mapping an academic major declaration linked to a student profile view.
/// </summary>
public record StudentMajorDto
{
    /// <summary>
    /// Gets the unique lookup identifier for the master major asset catalog row.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the descriptive name or title of the academic major program (e.g., "Software Engineering").
    /// </summary>
    public string Name { get; init; } = string.Empty;
}

/// <summary>
/// Data Transfer Object mapping an education sub-specialty tracking row linked to a student profile view.
/// </summary>
public record StudentSpecialtyDto
{
    /// <summary>
    /// Gets the unique lookup identifier for the fine-grained specialty track catalog row.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the descriptive name or title of the sub-specialty track (e.g., "Cloud Engineering").
    /// </summary>
    public string Name { get; init; } = string.Empty;
}

/// <summary>
/// Data Transfer Object mapping a specific skill competency claimed within a student profile view.
/// </summary>
public record StudentSkillDto
{
    /// <summary>
    /// Gets the unique identity identifier targeting the master skill lookup directory.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the descriptive name or title of the competency area (e.g., "C# Programming").
    /// </summary>
    public string Name { get; init; } = string.Empty;
}
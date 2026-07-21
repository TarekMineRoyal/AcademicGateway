using MediatR;
using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.Students.Commands.RegisterStudent;

/// <summary>
/// CQRS Command to register a new student account profile within the academic gateway.
/// Provisions baseline user identity credentials and initializes an encapsulated Student aggregate root.
/// </summary>
public record RegisterStudentCommand : IRequest<Guid>
{
    /// <summary>
    /// Gets the unique academic institutional or personal email address tracking the identity credential.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets the unique security username requested for authentication workflows.
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// Gets the plain-text password requested for credential configuration.
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Gets the formal legal full display name tracking this student's identity records.
    /// </summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the optional target graduation completion calendar year logged by the student.
    /// </summary>
    public int? GraduationYear { get; init; }

    /// <summary>
    /// Gets the optional summary or biography text describing this student profile for vector matchmaking.
    /// </summary>
    public string? AboutMe { get; init; }

    /// <summary>
    /// Gets the read-only collection of universal academic major tracking identifiers chosen by this student.
    /// </summary>
    public IReadOnlyCollection<Guid> MajorIds { get; init; } = Array.Empty<Guid>();

    /// <summary>
    /// Gets the read-only collection of fine-grained sub-track specialty identifiers chosen by this student.
    /// </summary>
    public IReadOnlyCollection<Guid> SpecialtyIds { get; init; } = Array.Empty<Guid>();

    /// <summary>
    /// Gets the read-only collection of technical capability or competency skill identifiers claimed by this student.
    /// </summary>
    public IReadOnlyCollection<Guid> SkillIds { get; init; } = Array.Empty<Guid>();
}
using MediatR;
using System;

namespace AcademicGateway.Application.Features.Professors.Commands.RegisterProfessor;

/// <summary>
/// CQRS Command to register a new institutional faculty member profile.
/// Provisions the underlying identity credentials and initializes an encapsulated Professor aggregate record.
/// </summary>
public record RegisterProfessorCommand : IRequest<Guid>
{
    /// <summary>
    /// Gets the unique corporate or academic institutional email address tracking the identity credential.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets the target unique security username identifier.
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// Gets the plain-text password requested for credential configuration.
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Gets the legal full name string matching this institutional faculty member.
    /// </summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the target academic department division designation text (e.g., "Computer Science").
    /// </summary>
    public string AcademicDepartment { get; init; } = string.Empty;

    /// <summary>
    /// Gets the professional rank title status holding for this faculty member (e.g., "Associate Professor").
    /// </summary>
    public string Rank { get; init; } = string.Empty;

    /// <summary>
    /// Gets the maximum ceiling boundary number of student projects this professor can supervise simultaneously.
    /// </summary>
    public int MaxSupervisionCapacity { get; init; }
}
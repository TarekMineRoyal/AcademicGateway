using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.Professors.Queries.GetPublicProfessorProfile;

/// <summary>
/// Data Transfer Object representing the public profile view of an institutional faculty member.
/// Decoupled from private/self profile endpoints for public lookups.
/// </summary>
public record GetPublicProfessorProfileQueryDto
{
    /// <summary>
    /// Gets the global unique entity identifier tracking this professor profile.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the legal full display name of the faculty member.
    /// </summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the corporate or institutional contact email address mapped to the account.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets the official department division designation text (e.g., "Computer Science").
    /// </summary>
    public string Department { get; init; } = string.Empty;

    /// <summary>
    /// Gets the professional rank title tier held by this faculty member (e.g., "Associate Professor").
    /// </summary>
    public string Rank { get; init; } = string.Empty;

    /// <summary>
    /// Gets the optional summary or biography narrative.
    /// </summary>
    public string? AboutMe { get; init; }

    /// <summary>
    /// Gets the collection of research interest topic area names mapped to this faculty member.
    /// </summary>
    public IReadOnlyCollection<string> ResearchInterests { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the maximum ceiling boundary number of student projects this professor can supervise concurrently.
    /// </summary>
    public int MaxSupervisionCapacity { get; init; }

    /// <summary>
    /// Gets the current count of active project allocations actively managed under this professor's guidance.
    /// </summary>
    public int CurrentProjectCount { get; init; }

    /// <summary>
    /// Gets a value indicating whether this professor possesses open allocation slots to accept new project supervisions.
    /// </summary>
    public bool IsAcceptingProjects { get; init; }
}
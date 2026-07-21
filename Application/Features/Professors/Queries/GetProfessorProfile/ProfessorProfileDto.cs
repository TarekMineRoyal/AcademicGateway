using System;

namespace AcademicGateway.Application.Features.Professors.Queries.GetProfessorProfile;

/// <summary>
/// Data Transfer Object representing the comprehensive profile view of an institutional faculty member.
/// Exposes rich domain properties, capacity boundaries, and current state metrics.
/// </summary>
public record ProfessorProfileDto
{
    /// <summary>
    /// Gets the global unique entity identifier tracking this professor profile, mapping 1:1 to their identity credentials.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the legal full display name of the faculty member.
    /// </summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the official department division designation text (e.g., "Computer Science").
    /// </summary>
    public string Department { get; init; } = string.Empty;

    /// <summary>
    /// Gets the professional rank title tier held by this faculty member (e.g., "Associate Professor").
    /// </summary>
    public string Rank { get; init; } = string.Empty;

    /// <summary>
    /// Gets the optional summary or biography narrative used for AI vector matchmaking and project recommendations.
    /// </summary>
    public string? AboutMe { get; init; }

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
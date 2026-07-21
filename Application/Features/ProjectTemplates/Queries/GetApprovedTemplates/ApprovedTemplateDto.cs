using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.ProjectTemplates.Queries.GetApprovedTemplates;

/// <summary>
/// Data Transfer Object representing an approved, publicly available project template blueprint.
/// Exposes partner information, required skill matrices, and optional academic alignments to students searching for placements.
/// </summary>
public record ApprovedTemplateDto
{
    /// <summary>
    /// Gets the unique entity identifier assigned to this specific project template.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the unique identifier of the corporate industry provider who owns this blueprint.
    /// </summary>
    public Guid ProviderId { get; init; }

    /// <summary>
    /// Gets the registered corporate name of the hosting organization (e.g., "Tech Corp").
    /// </summary>
    public string ProviderCompanyName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the headline title description of the project blueprint.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the detailed text description outlining requirements, execution scopes, and deliverables.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the optional unique identifier targeting an academic major domain restriction.
    /// </summary>
    public Guid? MajorId { get; init; }

    /// <summary>
    /// Gets the optional unique identifier targeting an academic specialty domain restriction.
    /// </summary>
    public Guid? SpecialtyId { get; init; }

    /// <summary>
    /// Gets the optional descriptive name of the targeted academic major.
    /// </summary>
    public string? MajorName { get; init; }

    /// <summary>
    /// Gets the optional descriptive name of the targeted academic specialty.
    /// </summary>
    public string? SpecialtyName { get; init; }

    /// <summary>
    /// Gets the read-only collection of required skill competencies mapped as prerequisites for student placement matching.
    /// </summary>
    public IReadOnlyCollection<TemplateSkillDto> Skills { get; init; } = Array.Empty<TemplateSkillDto>();
}

/// <summary>
/// Data Transfer Object representing a specific skill prerequisite mapped to an approved project blueprint.
/// </summary>
public record TemplateSkillDto
{
    /// <summary>
    /// Gets the unique identity identifier targeting the master skill asset lookup record.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the descriptive name or title of the competency area (e.g., "C# Programming").
    /// </summary>
    public string Name { get; init; } = string.Empty;
}
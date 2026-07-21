using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Common.Models.AiSync;

/// <summary>
/// Fat DTO payload sent to POST /api/v1/sync/project for AI vector matchmaking index synchronization.
/// </summary>
public class ProjectSyncModel
{
    /// <summary>
    /// Gets or sets the nested project template entity representation.
    /// </summary>
    public ProjectTemplatePayload ProjectTemplate { get; set; } = new();

    /// <summary>
    /// Gets or sets the resolved text label for the target academic major.
    /// </summary>
    public string? MajorName { get; set; }

    /// <summary>
    /// Gets or sets the resolved text label for the target academic specialty.
    /// </summary>
    public string? SpecialtyName { get; set; }

    /// <summary>
    /// Gets or sets the resolved text labels for required technical skills.
    /// </summary>
    public List<string> SkillNames { get; set; } = new();
}

/// <summary>
/// Nested project template domain data within the sync contract.
/// </summary>
public class ProjectTemplatePayload
{
    /// <summary>
    /// Gets or sets the project template unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the headline title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider account identifier.
    /// </summary>
    public Guid ProviderId { get; set; }

    /// <summary>
    /// Gets or sets the template creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the list of required skill identifiers.
    /// </summary>
    public List<Guid> SkillIds { get; set; } = new();

    /// <summary>
    /// Gets or sets the target academic major identifier.
    /// </summary>
    public Guid? MajorId { get; set; }

    /// <summary>
    /// Gets or sets the target academic specialty identifier.
    /// </summary>
    public Guid? SpecialtyId { get; set; }
}
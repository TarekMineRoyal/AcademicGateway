using System;

namespace AcademicGateway.Application.Features.ProjectTemplates.Queries.GetPendingProjectTemplates;

/// <summary>
/// Data Transfer Object representing an industry-proposed project blueprint template awaiting clearance.
/// Exposes the operational metrics and layout information necessary for structural administrative review.
/// </summary>
public record PendingProjectTemplateDto
{
    /// <summary>
    /// Gets the unique identifier of the project template.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the title of the proposed academic or industry project template.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the detailed description outlining the core objectives and deliverables of the template blueprint.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the corporate or industry name of the provider organization that authored the template proposal.
    /// </summary>
    public string ProviderName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the chronological timestamp specifying exactly when the template was submitted for system clearance.
    /// </summary>
    public DateTime SubmittedAt { get; init; }
}
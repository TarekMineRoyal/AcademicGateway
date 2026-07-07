using System;

namespace AcademicGateway.Application.Features.ProviderApplications.Queries.GetPendingProviderApplications;

/// <summary>
/// Data Transfer Object representing a provider application awaiting review.
/// Exposes critical identity metadata and submission criteria required for operational evaluation.
/// </summary>
public record PendingProviderApplicationDto
{
    /// <summary>
    /// Gets the unique identifier of the provider application.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the legal name of the provider organization or individual applying for academic onboarding.
    /// </summary>
    public string ProviderName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the primary point of contact email address for communication regarding verification.
    /// </summary>
    public string ContactEmail { get; init; } = string.Empty;

    /// <summary>
    /// Gets the brief description or summary of academic and industry credentials submitted for evaluation.
    /// </summary>
    public string CredentialsSummary { get; init; } = string.Empty;

    /// <summary>
    /// Gets the timestamp when the application was finalized and submitted by the provider.
    /// </summary>
    public DateTime SubmittedAt { get; init; }
}
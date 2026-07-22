using System;
using AcademicGateway.Domain.Providers.Enums;

namespace AcademicGateway.Application.Features.ProviderApplications.Queries.GetProviderApplicationById;

/// <summary>
/// Data Transfer Object representing full details of a provider application.
/// Exposes attached verification documents, firm credentials, current status, and evaluation history.
/// </summary>
public record ProviderApplicationDto
{
    /// <summary>
    /// Gets the unique identifier of the provider application.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the unique identifier of the applicant provider user profile.
    /// </summary>
    public Guid ProviderId { get; init; }

    /// <summary>
    /// Gets the registered corporate or organizational name of the provider.
    /// </summary>
    public string CompanyName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the full operational overview and academic/industry credentials submitted for review.
    /// </summary>
    public string CompanyDetails { get; init; } = string.Empty;

    /// <summary>
    /// Gets the remote storage link containing attached corporate verification paperwork.
    /// </summary>
    public string VerificationDocumentsUrl { get; init; } = string.Empty;

    /// <summary>
    /// Gets the current state within the evaluation lifecycle pipeline.
    /// </summary>
    public ProviderApplicationStatus Status { get; init; }

    /// <summary>
    /// Gets the identifier of the reviewer processing this application, if evaluated.
    /// </summary>
    public Guid? ReviewedById { get; init; }

    /// <summary>
    /// Gets the display or full name of the evaluating reviewer, if evaluated.
    /// </summary>
    public string? ReviewerName { get; init; }

    /// <summary>
    /// Gets the explanatory justification provided if the application was rejected.
    /// </summary>
    public string? RejectionReason { get; init; }

    /// <summary>
    /// Gets the timestamp when the application record was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the timestamp when the review evaluation was committed, if applicable.
    /// </summary>
    public DateTime? ReviewedAt { get; init; }

    /// <summary>
    /// Gets the structured audit history tracking submission and review events.
    /// </summary>
    public ProviderApplicationHistoryDto History { get; init; } = new();
}

/// <summary>
/// Sub-DTO carrying lifecycle evaluation and audit history for the provider application.
/// </summary>
public record ProviderApplicationHistoryDto
{
    /// <summary>
    /// Gets the timestamp when the application was initially submitted.
    /// </summary>
    public DateTime SubmittedAt { get; init; }

    /// <summary>
    /// Gets the timestamp when the review decision was finalized.
    /// </summary>
    public DateTime? EvaluatedAt { get; init; }

    /// <summary>
    /// Gets the identifier of the reviewer who evaluated the application.
    /// </summary>
    public Guid? EvaluatedById { get; init; }

    /// <summary>
    /// Gets the reviewer name attached to the evaluation record.
    /// </summary>
    public string? EvaluatedByName { get; init; }

    /// <summary>
    /// Gets the rejection rationale recorded during evaluation, if applicable.
    /// </summary>
    public string? RejectionReason { get; init; }
}
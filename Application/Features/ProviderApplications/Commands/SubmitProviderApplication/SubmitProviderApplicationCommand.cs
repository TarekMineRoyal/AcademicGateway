using MediatR;
using System;

namespace AcademicGateway.Application.Features.ProviderApplications.Commands.SubmitProviderApplication;

/// <summary>
/// CQRS Command to submit or resubmit an onboarding verification application for a corporate provider profile.
/// Triggers the initialization of a new evaluation cycle within the compliance audit pipeline.
/// </summary>
public record SubmitProviderApplicationCommand : IRequest<Guid>
{
    /// <summary>
    /// Gets the unique identifier primary key of the target provider account executing the submission.
    /// Maps 1:1 to the underlying corporate profile record requesting platform verification.
    /// </summary>
    public Guid ProviderId { get; init; }

    /// <summary>
    /// Gets the descriptive operational background, capability statements, and focus summary details of the partner company.
    /// </summary>
    public string CompanyDetails { get; init; } = string.Empty;

    /// <summary>
    /// Gets the secure reference URI locating corporate verification paperwork, business registration records, or legal credentials.
    /// </summary>
    public string VerificationDocumentsUrl { get; init; } = string.Empty;
}
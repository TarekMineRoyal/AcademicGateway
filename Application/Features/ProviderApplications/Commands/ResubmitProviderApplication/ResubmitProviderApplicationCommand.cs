using MediatR;
using System;

namespace AcademicGateway.Application.Features.ProviderApplications.Commands.ResubmitProviderApplication;

/// <summary>
/// CQRS Command to update and resubmit an onboarding verification application for a corporate provider profile.
/// Invokes corrections on a previously rejected compliance record and shifts it back into the active institutional evaluation pool.
/// </summary>
public record ResubmitProviderApplicationCommand : IRequest<Guid>
{
    /// <summary>
    /// Gets the unique identifier tracking code mapping back to the account profile executing the resubmission.
    /// Maps 1:1 to the underlying corporate profile record requesting platform verification.
    /// </summary>
    public Guid ProviderId { get; init; }

    /// <summary>
    /// Gets the corrected or refined descriptive operational background and focus summary details of the partner company.
    /// </summary>
    public string CompanyDetails { get; init; } = string.Empty;

    /// <summary>
    /// Gets the new or updated reference URI locating corporate verification paperwork, business registration records, or legal credentials.
    /// </summary>
    public string VerificationDocumentsUrl { get; init; } = string.Empty;
}
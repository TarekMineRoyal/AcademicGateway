using MediatR;
using System;

namespace AcademicGateway.Application.Features.Providers.Commands.SubmitApplication;

/// <summary>
/// Represents the transactional CQRS command intent to submit or resubmit an onboarding verification application for a corporate provider profile.
/// </summary>
public record SubmitProviderApplicationCommand : IRequest<Guid>
{
    /// <summary>
    /// Gets the strongly-typed identifier primary key of the target provider account.
    /// </summary>
    public Guid ProviderId { get; init; } // Fixed: Changed from string to Guid to enforce type safety at the boundary

    /// <summary>
    /// Gets the descriptive operational background and focus summary details of the partner company.
    /// </summary>
    public string CompanyDetails { get; init; } = string.Empty;

    /// <summary>
    /// Gets the secure reference URI locating corporate verification paperwork or legal credentials.
    /// </summary>
    public string VerificationDocumentsUrl { get; init; } = string.Empty;
}
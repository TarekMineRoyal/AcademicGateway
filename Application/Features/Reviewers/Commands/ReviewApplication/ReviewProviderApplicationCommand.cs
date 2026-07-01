using MediatR;
using System;

namespace AcademicGateway.Application.Features.Reviewers.Commands.ReviewApplication;

/// <summary>
/// CQRS Command to record an administrative quality assurance or compliance decision on a pending corporate Provider enrollment application.
/// Triggers terminal status state machine modifications within the target application record boundary.
/// </summary>
public record ReviewProviderApplicationCommand : IRequest
{
    /// <summary>
    /// Gets the unique tracking identifier of the provider application undergoing active evaluation.
    /// </summary>
    public Guid ApplicationId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the auditing reviewer processing the transaction.
    /// Maps 1:1 to their underlying centralized user identity authentication record.
    /// </summary>
    public Guid ReviewerId { get; init; }

    /// <summary>
    /// Gets a value indicating whether the corporate provider profile application passes compliance and is authorized for active platform access.
    /// </summary>
    public bool IsApproved { get; init; }

    /// <summary>
    /// Gets the formal feedback or compliance notes explaining a negative evaluation decision.
    /// This parameter is strictly required when <see cref="IsApproved"/> evaluates to <c>false</c>.
    /// </summary>
    public string? RejectionReason { get; init; }
}
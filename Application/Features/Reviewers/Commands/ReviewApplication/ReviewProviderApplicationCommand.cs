using MediatR;
using System;

namespace AcademicGateway.Application.Features.Reviewers.Commands.ReviewApplication;

/// <summary>
/// A type-safe request command payload used to record a reviewer's decision regarding a provider application.
/// </summary>
public record ReviewProviderApplicationCommand : IRequest
{
    public Guid ApplicationId { get; init; }
    
    /// <summary>
    /// Gets the unique Identity User identifier of the reviewer processing the action.
    /// Unified strictly as a <see cref="Guid"/> to match the underlying security context database definitions.
    /// </summary>
    public Guid ReviewerIdentityUserId { get; init; } 
    
    public bool IsApproved { get; init; }
    public string? RejectionReason { get; init; }
}
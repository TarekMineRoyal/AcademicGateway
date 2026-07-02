using MediatR;
using System;

namespace AcademicGateway.Application.Features.TechSupportProposals.Commands.ReviewTechSupportProposal;

/// <summary>
/// CQRS Command for a student workspace owner to accept or decline a corporate technical support mentor proposal.
/// </summary>
public record ReviewTechSupportProposalCommand : IRequest<Unit>
{
    /// <summary>
    /// Gets the unique tracking identifier of the parent project instance workspace.
    /// </summary>
    public Guid ProjectInstanceId { get; init; }

    /// <summary>
    /// Gets the unique tracking identifier of the corporate mentorship proposal being evaluated.
    /// </summary>
    public Guid TechSupportProposalId { get; init; }

    /// <summary>
    /// Gets a value indicating whether the student accepts or declines the assistance offer.
    /// True indicates acceptance; False indicates rejection.
    /// </summary>
    public bool Accept { get; init; }

    /// <summary>
    /// Gets optional feedback text or reasons indicating why a support attachment offer was declined.
    /// </summary>
    public string? RejectionReason { get; init; }
}
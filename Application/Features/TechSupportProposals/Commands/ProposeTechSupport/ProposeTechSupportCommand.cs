using MediatR;
using System;

namespace AcademicGateway.Application.Features.TechSupportProposals.Commands.ProposeTechSupport;

/// <summary>
/// CQRS Command for a corporate industry provider to propose an explicitly spawned 
/// Tech Support account as a technical mentor to a running student project instance.
/// Includes mandatory contextual commentary to introduce the support staff member.
/// </summary>
public record ProposeTechSupportCommand : IRequest<Guid>
{
    /// <summary>
    /// Gets the unique tracking identifier of the student project instance workspace receiving the assistance.
    /// </summary>
    public Guid ProjectInstanceId { get; init; }

    /// <summary>
    /// Gets the unique tracking identifier of the corporate tech support user profile assigned to mentor.
    /// </summary>
    public Guid TechSupportAccountId { get; init; }

    /// <summary>
    /// Gets the introductory statement or context detailing how this specific support specialist 
    /// will assist the student with their technical challenges.
    /// </summary>
    public string Message { get; init; } = string.Empty;
}
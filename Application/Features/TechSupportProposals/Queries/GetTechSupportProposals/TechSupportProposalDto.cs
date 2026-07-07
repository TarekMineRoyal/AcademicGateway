using AcademicGateway.Domain.ProjectInstances.Enums;
using System;

namespace AcademicGateway.Application.Features.TechSupportProposals.Queries.GetTechSupportProposals;

/// <summary>
/// Data transfer object representing a corporate technical assistance engagement offer.
/// Exposes a flattened read-only snapshot for students or supervisors to evaluate tech support proposals.
/// </summary>
public class TechSupportProposalDto
{
    /// <summary>
    /// Gets or sets the unique tracking identifier for the corporate proposal record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the parent project instance identifier receiving the technical aid.
    /// </summary>
    public Guid ProjectInstanceId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier code tracking the corporate tech support account profile.
    /// </summary>
    public Guid TechSupportAccountId { get; set; }

    /// <summary>
    /// Gets or sets the current processing evaluation state of this provider offer.
    /// </summary>
    public TechSupportProposalStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the context commentary if the proposal is declined.
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Gets or sets the precise timestamp recording when this proposal record was emitted.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
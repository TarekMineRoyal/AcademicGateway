using System;
using AcademicGateway.Domain.Common;
using AcademicGateway.Domain.ProjectInstances.Enums;
using AcademicGateway.Domain.Providers;

namespace AcademicGateway.Domain.ProjectInstances;

/// <summary>
/// Tracks an industry mentorship arrangement where a provider offers to link a corporate 
/// Tech Support user into a student's active instance workspace to aid execution.
/// </summary>
public class TechSupportProposal : BaseEntity
{
    /// <summary>
    /// Gets the unique identifier for the technical support proposal record.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the parent project instance ID receiving the technical aid.
    /// </summary>
    public Guid ProjectInstanceId { get; private set; }

    /// <summary>
    /// Gets the unique identifier code tracking the corporate tech support account profile.
    /// </summary>
    public Guid TechSupportAccountId { get; private set; }

    /// <summary>
    /// Gets the current processing evaluation state of this provider offer.
    /// </summary>
    public TechSupportProposalStatus Status { get; internal set; }

    /// <summary>
    /// Gets context commentary if the student chooses to decline the corporate mentorship assignment.
    /// </summary>
    public string? RejectionReason { get; internal set; }

    /// <summary>
    /// Gets the precise timestamp recording when this proposal record was emitted.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Gets the navigation property back to the containing project workspace workspace.
    /// </summary>
    public ProjectInstance ProjectInstance { get; private set; } = null!;

    /// <summary>
    /// Gets the navigation property tracking the corporate mentorship account.
    /// </summary>
    public TechSupportAccount TechSupportAccount { get; private set; } = null!;

    /// <summary>
    /// EF Core constructor requirement for database hydration.
    /// </summary>
    private TechSupportProposal()
    {
    }

    /// <summary>
    /// Initializes a new instance of a corporate tech support engagement offer.
    /// </summary>
    internal TechSupportProposal(Guid projectInstanceId, Guid techSupportAccountId, DateTime createdAt)
    {
        Id = Guid.NewGuid();
        ProjectInstanceId = projectInstanceId;
        TechSupportAccountId = techSupportAccountId;
        Status = TechSupportProposalStatus.Pending;
        CreatedAt = createdAt;
    }
}
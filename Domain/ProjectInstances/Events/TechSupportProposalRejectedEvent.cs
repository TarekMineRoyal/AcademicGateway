using System;
using AcademicGateway.Domain.Common;

namespace AcademicGateway.Domain.ProjectInstances.Events;

/// <summary>
/// Raised when a student declines a corporate provider's technical mentorship proposal, 
/// signaling that the corporate provider account should be alerted of the refusal.
/// </summary>
public record TechSupportProposalRejectedEvent(
    Guid TechSupportProposalId,
    Guid ProjectInstanceId,
    Guid TechSupportAccountId,
    string? Reason) : IDomainEvent;
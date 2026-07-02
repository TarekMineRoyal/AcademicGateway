using System;
using AcademicGateway.Domain.Common;

namespace AcademicGateway.Domain.ProjectInstances.Events;

/// <summary>
/// Raised when a corporate provider proposes an industry mentor (Tech Support account) 
/// to assist a student within a specific active project instance workspace.
/// </summary>
public record TechSupportProposalCreatedEvent(
    Guid TechSupportProposalId,
    Guid ProjectInstanceId,
    Guid TechSupportAccountId) : IDomainEvent;
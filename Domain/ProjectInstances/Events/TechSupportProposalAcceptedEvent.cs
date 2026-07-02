using System;
using AcademicGateway.Domain.Common;

namespace AcademicGateway.Domain.ProjectInstances.Events;

/// <summary>
/// Raised when a student accepts a corporate technical mentor, unlocking tracking 
/// and commenting access for that tech support account.
/// </summary>
public record TechSupportProposalAcceptedEvent(
    Guid TechSupportProposalId,
    Guid ProjectInstanceId,
    Guid TechSupportAccountId) : IDomainEvent;
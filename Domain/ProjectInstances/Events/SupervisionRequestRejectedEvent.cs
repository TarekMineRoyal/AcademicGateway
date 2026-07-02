using System;
using AcademicGateway.Domain.Common;

namespace AcademicGateway.Domain.ProjectInstances.Events;

/// <summary>
/// Raised when a professor declines a student's supervision invitation, signaling that 
/// the student should be notified and the project workspace should unlock alternative matchmaking options.
/// </summary>
public record SupervisionRequestRejectedEvent(
    Guid SupervisionRequestId,
    Guid ProjectInstanceId,
    Guid ProfessorId,
    string? Reason) : IDomainEvent;
using System;
using AcademicGateway.Domain.Common;

namespace AcademicGateway.Domain.ProjectInstances.Events;

/// <summary>
/// Raised when a professor accepts an academic supervision invitation, officially signaling 
/// that the project instance state must be activated or updated with supervisory permissions.
/// </summary>
public record SupervisionRequestAcceptedEvent(
    Guid SupervisionRequestId,
    Guid ProjectInstanceId,
    Guid ProfessorId) : IDomainEvent;
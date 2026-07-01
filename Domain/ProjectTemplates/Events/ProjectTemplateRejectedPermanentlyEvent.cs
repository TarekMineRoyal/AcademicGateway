using System;
using Domain.Common;

namespace Domain.ProjectTemplates.Events;

/// <summary>
/// Represents the immutable domain event raised when an academic auditor permanently denies 
/// a project blueprint submission, locking it from subsequent updates.
/// </summary>
/// <remarks>
/// This event signals a terminal failure state transition to <c>Rejected</c>. It is crucial for administrative 
/// tracking, archiving compliance logs, and notifying partners regarding hard institutional alignment conflicts.
/// </remarks>
/// <param name="TemplateId">The unique identifier of the permanently rejected template.</param>
/// <param name="ProviderId">The unique identifier of the creating industry partner account.</param>
/// <param name="Reason">The administrative justification logged for executing the hard denial.</param>
public record ProjectTemplateRejectedPermanentlyEvent(Guid TemplateId, Guid ProviderId, string Reason) : IDomainEvent;
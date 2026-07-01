using System;
using Domain.Common;

namespace Domain.ProjectTemplates.Events;

/// <summary>
/// Represents the immutable domain event raised when a faculty reviewer flags a project 
/// blueprint as requiring specific corrections or text adjustments.
/// </summary>
/// <remarks>
/// This event indicates a collaborative feedback loop transition to <c>ChangesRequested</c>. 
/// It can be listened to by notification workers to send actionable revision instructions directly to the creator.
/// </remarks>
/// <param name="TemplateId">The unique identifier of the project template pushed back for revision.</param>
/// <param name="ProviderId">The unique identifier of the targeted corporate provider profile.</param>
/// <param name="Feedback">The logged instructional commentary detailing the required corrections.</param>
public record ProjectTemplateChangesRequestedEvent(Guid TemplateId, Guid ProviderId, string Feedback) : IDomainEvent;
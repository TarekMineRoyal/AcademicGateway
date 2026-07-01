using System;
using Domain.Common;

namespace Domain.ProjectTemplates.Events;

/// <summary>
/// Represents the immutable domain event raised when an industry partner formally submits 
/// a drafted or revised project blueprint into the faculty review pool.
/// </summary>
/// <remarks>
/// This event indicates a state transition to <c>PendingReview</c>. It is typically used to trigger 
/// out-of-band application processes, such as notifying academic reviewers or populating 
/// administrative quality compliance dashboards.
/// </remarks>
/// <param name="TemplateId">The unique identifier of the project template pushed into the review pipeline.</param>
/// <param name="ProviderId">The unique identifier of the owner industry partner profile.</param>
public record ProjectTemplateSubmittedEvent(Guid TemplateId, Guid ProviderId) : IDomainEvent;
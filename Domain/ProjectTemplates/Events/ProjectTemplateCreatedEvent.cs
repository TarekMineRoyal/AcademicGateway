using System;
using Domain.Common;

namespace Domain.ProjectTemplates.Events;

/// <summary>
/// Represents the immutable domain event raised when a new reusable project blueprint 
/// is initially instantiated and drafted by an industry partner.
/// </summary>
/// <remarks>
/// This event marks the beginning of a template's curation pipeline. It can be captured 
/// to trigger audit logging, track provider productivity metrics, or prime local read-model caches.
/// </remarks>
/// <param name="TemplateId">The unique identifier of the newly instantiated project template.</param>
/// <param name="ProviderId">The unique identifier of the industry partner account creating the blueprint.</param>
/// <param name="Title">The initial headline promo title assigned to the blueprint specification.</param>
public record ProjectTemplateCreatedEvent(Guid TemplateId, Guid ProviderId, string Title) : IDomainEvent;
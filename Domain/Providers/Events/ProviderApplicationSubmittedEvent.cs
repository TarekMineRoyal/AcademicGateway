using System;
using AcademicGateway.Domain.Common;

namespace AcademicGateway.Domain.Providers.Events;

/// <summary>
/// Represents the immutable domain event raised when an industry partner formally pushes 
/// their drafting compilation into the centralized faculty verification pool.
/// </summary>
/// <remarks>
/// Shifting the application status to <c>PendingReview</c> requires alerting our institutional 
/// compliance reviewer directory. This event decouples the provider-facing execution loop from 
/// the background notification engine or management dashboards.
/// </remarks>
/// <param name="ApplicationId">The unique identifier of the provider application moving into active evaluation pools.</param>
/// <param name="ProviderId">The unique identifier of the corporate partner seeking platform entry verification.</param>
public record ProviderApplicationSubmittedEvent(Guid ApplicationId, Guid ProviderId) : IDomainEvent;
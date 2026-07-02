using System;
using AcademicGateway.Domain.Common;

namespace AcademicGateway.Domain.Providers.Events;

/// <summary>
/// Represents the immutable domain event raised when a corporate contact overwrites a previously 
/// rejected configuration with updated documents or details and pushes it back for validation.
/// </summary>
/// <remarks>
/// This event indicates a critical collaborative iteration loop occurrence. It can be captured to track 
/// total processing cycle times, monitor aggregate application friction parameters, or update reviewer task queues.
/// </remarks>
/// <param name="ApplicationId">The unique identifier of the resubmitted onboarding application model row.</param>
/// <param name="ProviderId">The unique identifier of the corporate provider adjusting their details.</param>
public record ProviderApplicationResubmittedEvent(Guid ApplicationId, Guid ProviderId) : IDomainEvent;
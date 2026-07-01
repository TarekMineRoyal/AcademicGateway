using System;
using Domain.Common;

namespace Domain.Providers.Events;

/// <summary>
/// Represents the immutable domain event raised when a new stateful provider onboarding application 
/// context is initially initialized as a draft layout.
/// </summary>
/// <remarks>
/// This event marks the initial step of the corporate onboarding tunnel. It can be captured by auditing tracks, 
/// operational lifecycle analytics, or security monitoring loops to track abandoned or slow-filling applications.
/// </remarks>
/// <param name="ApplicationId">The unique identifier of the newly instantiated onboarding application tracking row.</param>
/// <param name="ProviderId">The unique identifier of the corporate partner account spawning the registration draft.</param>
public record ProviderApplicationCreatedEvent(Guid ApplicationId, Guid ProviderId) : IDomainEvent;
using System;
using Domain.Common;

namespace Domain.Providers.Events;

/// <summary>
/// Represents the immutable domain event raised when a provider's onboarding application 
/// is successfully reviewed and approved by an institutional administrator or faculty auditor.
/// </summary>
/// <remarks>
/// This event is used to trigger secondary, cross-aggregate side effects (such as updating 
/// the corresponding <see cref="Provider"/> profile's verification status) while maintaining 
/// transactional boundaries and aggregate isolation.
/// </remarks>
/// <param name="ProviderId">The unique identifier of the corporate provider associated with the approved application.</param>
public record ProviderApplicationApprovedEvent(Guid ProviderId) : IDomainEvent;
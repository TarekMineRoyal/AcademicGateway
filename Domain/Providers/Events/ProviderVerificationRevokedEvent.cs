using System;
using Domain.Common;

namespace Domain.Providers.Events;

/// <summary>
/// Represents the immutable domain event raised when an institutional authority manually revokes 
/// a corporate partner's certified status due to compliance violations or structural adjustments.
/// </summary>
/// <remarks>
/// This is an essential security boundary event. Elevating this occurrence ensures that application-tier handlers 
/// can instantly cascade security locks downstream—such as freeze-locking or hiding all public-facing project 
/// blueprints owned by this organization—preserving institutional platform safety.
/// </remarks>
/// <param name="ProviderId">The unique tracking identifier of the corporate provider account being frozen.</param>
public record ProviderVerificationRevokedEvent(Guid ProviderId) : IDomainEvent;
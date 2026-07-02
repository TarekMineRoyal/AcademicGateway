using System;
using AcademicGateway.Domain.Common;

namespace AcademicGateway.Domain.Providers.Events;

/// <summary>
/// Represents the immutable domain event raised when an institutional administrator or compliance auditor 
/// rejects an onboarding submission due to documentation gaps or validation anomalies.
/// </summary>
/// <remarks>
/// This event carries the precise administrative explanation context justifying the failure state. It is crucial 
/// for triggering immediate out-of-band feedback communications (such as mailing instructions) directly 
/// back onto the partner workspace without blocking the auditor's request transaction.
/// </remarks>
/// <param name="ApplicationId">The unique identifier of the evaluated onboarding application.</param>
/// <param name="ProviderId">The unique identifier of the corporate partner account receiving the denial.</param>
/// <param name="ReviewerId">The unique identifier tracking the evaluating compliance reviewer profile.</param>
/// <param name="Reason">The explanatory commentary framing why the documentation was deemed insufficient.</param>
public record ProviderApplicationRejectedEvent(Guid ApplicationId, Guid ProviderId, Guid ReviewerId, string Reason) : IDomainEvent;
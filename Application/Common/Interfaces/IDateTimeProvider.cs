using System;

namespace AcademicGateway.Application.Common.Interfaces;

/// <summary>
/// Defines the centralized chronological clock contract to decouple application-tier use cases 
/// from physical execution container hardware system runtime clocks. 
/// Guarantees deterministic evaluation and isolation of temporal business rules and state machine invariants.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current state-machine validated date and time expressed in Coordinated Universal Time (UTC).
    /// </summary>
    DateTime UtcNow { get; }
}
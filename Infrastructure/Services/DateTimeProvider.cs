using AcademicGateway.Application.Common.Interfaces;
using System;

namespace AcademicGateway.Infrastructure.Services;

/// <summary>
/// Provides a concrete execution wrapper over the physical operating system runtime environment clock.
/// Implements the <see cref="IDateTimeProvider"/> interface to enable deterministic chronological tracking.
/// </summary>
public class DateTimeProvider : IDateTimeProvider
{
    /// <summary>
    /// Gets the current date and time of the physical machine host environment, 
    /// explicitly calibrated to Coordinated Universal Time (UTC).
    /// </summary>
    public DateTime UtcNow => DateTime.UtcNow;
}
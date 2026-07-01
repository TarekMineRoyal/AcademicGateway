using System;

namespace AcademicGateway.Application.Common.Interfaces;

/// <summary>
/// Provides access to the identity context of the user currently executing the request.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the unique identifier of the currently authenticated user.
    /// Returns null if the user is not authenticated or the ID cannot be resolved.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Indicates whether the current request is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }
}
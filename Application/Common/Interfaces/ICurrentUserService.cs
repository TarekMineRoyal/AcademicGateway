using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Common.Interfaces;

/// <summary>
/// Provides access to the security and identity context of the user currently executing the request.
/// Acts as an architectural boundary separating application use-cases from infrastructure-specific authentication contexts.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the unique identifier of the currently authenticated user context.
    /// </summary>
    /// <value>A unique <see cref="Guid"/> if authenticated; otherwise, <c>null</c>.</value>
    Guid? UserId { get; }

    /// <summary>
    /// Gets a value indicating whether the current request execution context is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Evaluates whether the currently authenticated user context belongs to a specific security role.
    /// </summary>
    /// <param name="roleName">The descriptive token framework name of the authorization role to validate (e.g., "Reviewer").</param>
    /// <returns><c>true</c> if the user carries the designated claim role token; otherwise, <c>false</c>.</returns>
    bool IsInRole(string roleName);

    /// <summary>
    /// Gets a read-only collection of all authorization role context claims mapped to the current authenticated security principal.
    /// </summary>
    IReadOnlyCollection<string> Roles { get; }
}
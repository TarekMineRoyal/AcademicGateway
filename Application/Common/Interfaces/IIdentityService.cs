using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Common.Interfaces;

/// <summary>
/// Defines the centralized abstraction contract for user credential provisioning, authentication, and security context routing.
/// Decouples the core Application workflows from platform-specific identity frameworks like ASP.NET Core Identity or external OAuth2/OIDC providers.
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Asynchronously provisions a new secure security account profile within the central identity store repository.
    /// </summary>
    /// <param name="userName">The unique login handle requested for user identification.</param>
    /// <param name="email">The unique corporate, institutional, or personal contact electronic mail address mapped to the account.</param>
    /// <param name="password">The raw, plain-text security password phrase undergoing structural complexity filtering and infrastructure hashing.</param>
    /// <returns>
    /// A structured asynchronous tracking tuple describing the transactional outcome:
    /// <list type="bullet">
    /// <item><description><c>Succeeded</c>: Evaluates to <c>true</c> if the credential context was generated cleanly without failure flags.</description></item>
    /// <item><description><c>UserId</c>: Holds the newly assigned globally unique tracker identifier allocated onto the provisioned identity record.</description></item>
    /// <item><description><c>Errors</c>: An enumerable collection mapping descriptive string outputs if security rules or lookup unique boundaries fail.</description></item>
    /// </list>
    /// </returns>
    Task<(bool Succeeded, Guid UserId, IEnumerable<string> Errors)> CreateUserAsync(string userName, string email, string password);

    /// <summary>
    /// Asynchronously challenges supplied identity account credentials against stored values to yield a secure platform session.
    /// </summary>
    /// <param name="email">The registered security context electronic mail identifier representing the target account handle.</param>
    /// <param name="password">The secret plain-text verification string passed for profile match confirmation.</param>
    /// <returns>A signed cryptographically encrypted JSON Web Token (JWT) bearer string if authentication clears; otherwise, <c>null</c>.</returns>
    Task<string?> AuthenticateAsync(string email, string password);
}
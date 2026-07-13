using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Common.Interfaces;

/// <summary>
/// Defines the centralized abstraction contract for user credential provisioning, authentication, and security context routing.
/// Decouples the core Application workflows from platform-specific identity frameworks like ASP.NET Core Identity or external OAuth2/OIDC providers.
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Asynchronously provisions a new secure security account profile within the central identity store repository 
    /// and maps it to a specific system application authorization role context.
    /// </summary>
    /// <param name="userName">The unique login handle requested for user identification.</param>
    /// <param name="email">The unique corporate, institutional, or personal contact electronic mail address mapped to the account.</param>
    /// <param name="password">The raw, plain-text security password phrase undergoing structural complexity filtering and infrastructure hashing.</param>
    /// <param name="role">The targeted platform-defined framework security role name (e.g., "Student", "Provider", "Professor", "TechSupport") to be assigned to the identity.</param>
    /// <returns>
    /// A structured asynchronous tracking tuple describing the transactional outcome:
    /// <list type="bullet">
    /// <item><description><c>Succeeded</c>: Evaluates to <c>true</c> if both the credential context and role assignment were generated cleanly without failure flags.</description></item>
    /// <item><description><c>UserId</c>: Holds the newly assigned globally unique tracker identifier allocated onto the provisioned identity record.</description></item>
    /// <item><description><c>Errors</c>: An enumerable collection mapping descriptive string outputs if security rules or lookup unique boundaries fail.</description></item>
    /// </list>
    /// </returns>
    Task<(bool Succeeded, Guid UserId, IEnumerable<string> Errors)> CreateUserAsync(string userName, string email, string password, string role);

    /// <summary>
    /// Asynchronously challenges supplied identity account credentials against stored values to yield a secure platform session.
    /// </summary>
    /// <param name="email">The registered security context electronic mail identifier representing the target account handle.</param>
    /// <param name="password">The secret plain-text verification string passed for profile match confirmation.</param>
    /// <returns>A signed cryptographically encrypted JSON Web Token (JWT) bearer string if authentication clears; otherwise, <c>null</c>.</returns>
    Task<string?> AuthenticateAsync(string email, string password);

    /// <summary>
    /// Asynchronously searches across user security identity account records where the assigned role context is matching 'Professor'.
    /// Filters profiles case-insensitively using partial text validation across full name metrics, electronic mail records, or login handles.
    /// </summary>
    /// <param name="searchTerm">The optional keyword phrase token used to evaluate matching boundary filters.</param>
    /// <param name="cancellationToken">Propagates notification that network database operations should be canceled.</param>
    /// <returns>An immutable read-only sequence containing matching lightweight presentational professor search records.</returns>
    Task<IReadOnlyCollection<Features.Professors.Queries.SearchProfessors.ProfessorSearchResultDto>> SearchProfessorsAsync(
        string? searchTerm,
        CancellationToken cancellationToken);
}
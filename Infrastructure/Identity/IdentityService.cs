using AcademicGateway.Application.Common.Extensions;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Common.Models;
using AcademicGateway.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Infrastructure.Identity;

/// <summary>
/// Infrastructure-tier adapter implementing the <see cref="IIdentityService"/> abstraction contract.
/// Plugs directly into ASP.NET Core Identity's security state machines, issues securely signed JWT bearer tokens,
/// and aggregates cross-cutting data lookups between identity profiles and business core entities.
/// </summary>
public class IdentityService(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    ApplicationDbContext dbContext) : IIdentityService
{
    /// <summary>
    /// Asynchronously provisions a new secure identity credential within the backing ASP.NET Core Identity storage provider
    /// and assigns them to an explicit system authorization role context.
    /// </summary>
    /// <param name="userName">The unique login handle requested for user identification.</param>
    /// <param name="email">The unique contact electronic mail address mapped to the account.</param>
    /// <param name="password">The raw, plain-text security password phrase undergoing infrastructural complexity filtering and hashing.</param>
    /// <param name="role">The targeted framework role name (e.g., "Student", "Provider", "Professor") to assign to the account context.</param>
    /// <returns>A structured asynchronous tuple detailing the success state, user identity surrogate key, and an iterable listing of descriptive framework error payloads.</returns>
    public async Task<(bool Succeeded, Guid UserId, IEnumerable<string> Errors)> CreateUserAsync(
        string userName,
        string email,
        string password,
        string role)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email
        };

        // 1. Create the secure login credentials
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return (
                false,
                Guid.Empty,
                result.Errors.Select(e => e.Description)
            );
        }

        // 2. Bind the newly registered user to the targeted system role context
        var roleResult = await userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            return (
                false,
                user.Id,
                roleResult.Errors.Select(e => e.Description)
            );
        }

        return (
            true,
            user.Id,
            Enumerable.Empty<string>()
        );
    }

    /// <summary>
    /// Challenges incoming email and password text parameters against recorded credential configurations.
    /// Issues an authenticated JSON Web Token (JWT) bearer key housing verified role claim identities upon successful validation steps.
    /// </summary>
    /// <param name="email">The registered security context electronic mail identifier representing the target account handle.</param>
    /// <param name="password">The secret plain-text verification string passed for profile match confirmation.</param>
    /// <returns>A cryptographically signed JWT bearer authorization string if validation passes cleanly; otherwise, <c>null</c>.</returns>
    public async Task<string?> AuthenticateAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null || !await userManager.CheckPasswordAsync(user, password))
        {
            return null; // Invalid credentials check fallback trigger
        }

        // Generate and configure the target symmetric key structure from the service environment settings
        var secret = configuration["JwtSettings:Secret"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Fetch all system roles mapped to this identity account profile
        var userRoles = await userManager.GetRolesAsync(user);

        // Formulate standard corporate claim payloads
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // The standard "role" string literal
        foreach (var role in userRoles)
        {
            claims.Add(new Claim("role", role));
        }

        // Construct the lifecycle parameter details for the token asset structure
        var token = new JwtSecurityToken(
            issuer: configuration["JwtSettings:Issuer"],
            audience: configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(configuration["JwtSettings:ExpiryMinutes"]!)),
            signingCredentials: creds
        );

        // Clear the default outbound mapping to ensure claims like "role" remain flat JSON keys
        System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Asynchronously searches across professor security identity account records using a case-insensitive keyword phrase match with pagination.
    /// Performs an optimal join projection between core domain aggregates and security database structures.
    /// </summary>
    /// <param name="searchTerm">The optional keyword phrase token used to evaluate matching boundary filters.</param>
    /// <param name="pageNumber">The 1-based index of the page to retrieve.</param>
    /// <param name="pageSize">The maximum number of items to retrieve per page.</param>
    /// <param name="cancellationToken">Propagates notification that network database operations should be canceled.</param>
    /// <returns>A paginated result containing matching lightweight presentational professor search records.</returns>
    public async Task<PaginatedResult<Application.Features.Professors.Queries.SearchProfessors.ProfessorSearchResultDto>> SearchProfessorsAsync(
        string? searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        // Establish an optimized relational join query between the profile boundaries and security records.
        // Joining against the Professors table implicitly handles scoping to the 'Professor' system role.
        var query = from professor in dbContext.Professors
                    join user in userManager.Users on professor.Id equals user.Id
                    select new { professor, user };

        // Apply string segment criteria filters conditionally only if a non-whitespace keyword parameter is supplied
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(x => x.professor.FullName.ToLower().Contains(lowerSearchTerm) ||
                                     x.user.Email!.ToLower().Contains(lowerSearchTerm) ||
                                     x.user.UserName!.ToLower().Contains(lowerSearchTerm));
        }

        // Execute untracked relational projection and apply pagination
        var projectedQuery = query
            .AsNoTracking()
            .OrderBy(x => x.professor.FullName)
            .Select(x => new Application.Features.Professors.Queries.SearchProfessors.ProfessorSearchResultDto
            {
                Id = x.professor.Id,
                FullName = x.professor.FullName,
                Email = x.user.Email!
            });

        return await projectedQuery.ToPaginatedListAsync(pageNumber, pageSize, cancellationToken);
    }

    /// <summary>
    /// Asynchronously searches across professor security identity account records returning a full collection.
    /// Used by internal domain recommendation services.
    /// </summary>
    public async Task<IReadOnlyCollection<Application.Features.Professors.Queries.SearchProfessors.ProfessorSearchResultDto>> SearchProfessorsAsync(
        string? searchTerm,
        CancellationToken cancellationToken)
    {
        var query = from professor in dbContext.Professors
                    join user in userManager.Users on professor.Id equals user.Id
                    select new { professor, user };

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(x => x.professor.FullName.ToLower().Contains(lowerSearchTerm) ||
                                     x.user.Email!.ToLower().Contains(lowerSearchTerm) ||
                                     x.user.UserName!.ToLower().Contains(lowerSearchTerm));
        }

        return await query
            .AsNoTracking()
            .Select(x => new Application.Features.Professors.Queries.SearchProfessors.ProfessorSearchResultDto
            {
                Id = x.professor.Id,
                FullName = x.professor.FullName,
                Email = x.user.Email!
            })
            .ToListAsync(cancellationToken);
    }
}
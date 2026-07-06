using AcademicGateway.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AcademicGateway.Infrastructure.Identity;

/// <summary>
/// Infrastructure-tier adapter implementing the <see cref="IIdentityService"/> abstraction contract.
/// Plugs directly into ASP.NET Core Identity's security state machines and issues securely signed JWT bearer tokens.
/// </summary>
public class IdentityService(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration) : IIdentityService
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

        // Inject each assigned role context as a standard ClaimTypes.Role metadata token
        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Construct the lifecycle parameter details for the token asset structure
        var token = new JwtSecurityToken(
            issuer: configuration["JwtSettings:Issuer"],
            audience: configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(configuration["JwtSettings:ExpiryMinutes"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
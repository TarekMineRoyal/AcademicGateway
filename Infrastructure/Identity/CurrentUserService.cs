using AcademicGateway.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace AcademicGateway.Infrastructure.Identity;

/// <summary>
/// Implementation of <see cref="ICurrentUserService"/> that extracts security principal identity metadata 
/// dynamically from the active HTTP request context claims.
/// </summary>
public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    /// <inheritdoc/>
    public Guid? UserId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;

            // Attempt to resolve from NameIdentifier (standard) or 'sub' (JWT/OIDC standard)
            var userIdString = user?.FindFirstValue(ClaimTypes.NameIdentifier)
                               ?? user?.FindFirst("sub")?.Value;

            return Guid.TryParse(userIdString, out var guid) ? guid : null;
        }
    }

    /// <inheritdoc/>
    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    /// <inheritdoc/>
    public bool IsInRole(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return false;
        }

        // Leveraging the built-in ClaimsPrincipal.IsInRole handles both custom claim mappings and standard roles securely
        return httpContextAccessor.HttpContext?.User?.IsInRole(roleName) ?? false;
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<string> Roles
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return Array.Empty<string>();
            }

            // Extract values matching both the legacy WS-Federation role claim uri and modern OIDC token 'role' strings safely
            return user.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value)
                .Distinct()
                .ToList()
                .AsReadOnly();
        }
    }
}
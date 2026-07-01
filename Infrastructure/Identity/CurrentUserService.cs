using AcademicGateway.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace AcademicGateway.Infrastructure.Identity;

/// <summary>
/// Implementation of <see cref="ICurrentUserService"/> that extracts identity context 
/// from the active HTTP request.
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
}
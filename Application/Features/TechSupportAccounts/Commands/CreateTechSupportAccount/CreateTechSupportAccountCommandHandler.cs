using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.Providers;
using AcademicGateway.Domain.Providers.Exceptions;
using AcademicGateway.Domain.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.TechSupportAccounts.Commands.CreateTechSupportAccount;

/// <summary>
/// Handles the transaction routine to process a <see cref="CreateTechSupportAccountCommand"/>.
/// </summary>
public class CreateTechSupportAccountCommandHandler(
    IApplicationDbContext context,
    IIdentityService identityService,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateTechSupportAccountCommand, Guid>
{
    /// <summary>
    /// Processes identity credential generation, enforces verification rules, instantiates the domain account, and flushes state securely.
    /// </summary>
    public async Task<Guid> Handle(CreateTechSupportAccountCommand request, CancellationToken cancellationToken)
    {
        // 1. Enforce active security session validation early before executing relational queries
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Access Denied: Authentication is mandatory to provision technical support accounts.");
        }

        // 2. Retrieve the managing Provider profile matching the requested context
        var provider = await context.Providers
            .FirstOrDefaultAsync(p => p.Id == request.ProviderId, cancellationToken);

        // 3. Cross-reference session user and shield resource existence
        // Verify tenancy alignment: The currently authenticated user ID must match the requested Provider target.
        // Uniformly throw an UnauthorizedAccessException if the record is missing OR owned by another provider profile.
        if (provider == null || request.ProviderId != currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Access Denied: The requested provider profile was not found, or you do not possess account provisioning authorization permissions.");
        }

        // 4. Enforce platform authorization boundaries using the strongly-typed domain exception
        if (!provider.IsVerified)
        {
            throw new ProviderNotVerifiedException(request.ProviderId);
        }

        // 5. Provision secure baseline application identity credentials
        var (succeeded, identityUserId, errors) = await identityService.CreateUserAsync(
            request.Email,     // userName
            request.Email,     // email
            request.Password,
            Roles.TechSupport); // password

        if (!succeeded)
        {
            throw new InvalidOperationException($"Failed to provision identity credentials: {string.Join(", ", errors)}");
        }

        // 6. Enforce Domain Encapsulation - Initialize via explicit parameterized constructor logic.
        var techAccount = new TechSupportAccount(
            id: identityUserId,
            providerId: request.ProviderId,
            staffNumber: request.StaffNumber,
            supportTier: request.SupportTier
        );

        // 7. Record the tracking entity into our persistence store
        context.TechSupportAccounts.Add(techAccount);
        await context.SaveChangesAsync(cancellationToken);

        // 8. Return the tracking key
        return techAccount.Id;
    }
}
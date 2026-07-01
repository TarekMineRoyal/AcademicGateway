using AcademicGateway.Application.Common.Interfaces;
using Domain.Providers;
using Domain.Providers.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.TechSupportAccounts.Commands.CreateTechSupportAccount;

/// <summary>
/// Handles the transaction routine to process a <see cref="CreateTechSupportAccountCommand"/>.
/// Verifies the provisioning authority of the managing provider, creates core identity credentials, 
/// and instantiates an encapsulated TechSupportAccount aggregate.
/// </summary>
public class CreateTechSupportAccountCommandHandler(
    IApplicationDbContext context,
    IIdentityService identityService)
    : IRequestHandler<CreateTechSupportAccountCommand, Guid>
{
    /// <summary>
    /// Processes identity credential generation, enforces verification rules, instantiates the domain account, and flushes state.
    /// </summary>
    /// <param name="request">The structural parameter bundle tracking managing credentials and support account metadata specifications.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>The newly assigned tracking Guid identifying the materialized TechSupportAccount aggregate root entity.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the specified managing provider reference is missing from database records.</exception>
    /// <exception cref="ProviderNotVerifiedException">Thrown if the requesting managing provider has not completed platform verification onboarding gates.</exception>
    /// <exception cref="InvalidOperationException">Thrown if security profile provisioning fails via credential infrastructure restrictions.</exception>
    /// <exception cref="InvalidTechSupportDetailsException">Thrown if core textual inputs (staff number, tier) violate domain invariant rules.</exception>
    public async Task<Guid> Handle(CreateTechSupportAccountCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate that the managing Provider exists and is currently verified
        // Aligned with the Guid domain key transformation (Id instead of UserId)
        var provider = await context.Providers
            .FirstOrDefaultAsync(p => p.Id == request.ProviderId, cancellationToken);

        if (provider == null)
        {
            throw new KeyNotFoundException($"Provider profile with ID '{request.ProviderId}' was not found.");
        }

        // 2. Enforce platform authorization boundaries using our strongly-typed domain exception
        if (!provider.IsVerified)
        {
            throw new ProviderNotVerifiedException(request.ProviderId);
        }

        // 3. Provision secure baseline application identity credentials
        var (succeeded, identityUserId, errors) = await identityService.CreateUserAsync(
            request.Email,     // userName
            request.Email,     // email
            request.Password); // password

        if (!succeeded)
        {
            throw new InvalidOperationException($"Failed to provision identity credentials: {string.Join(", ", errors)}");
        }

        // 4. Enforce Domain Encapsulation - Initialize via explicit parameterized constructor logic.
        // identityUserId is already a strongly typed Guid coming back from IIdentityService.
        var techAccount = new TechSupportAccount(
            id: identityUserId,
            staffNumber: request.StaffNumber,
            supportTier: request.SupportTier
        );

        // 5. Record the tracking entity into our persistence store
        context.TechSupportAccounts.Add(techAccount);
        await context.SaveChangesAsync(cancellationToken);

        // 6. Return the tracking key
        return techAccount.Id;
    }
}
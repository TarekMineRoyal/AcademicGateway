using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.TechSupportAccounts.Commands.CreateTechSupportAccount;

public class CreateTechSupportAccountCommandHandler(
    IApplicationDbContext context,
    IIdentityService identityService)
    : IRequestHandler<CreateTechSupportAccountCommand, Guid>
{
    public async Task<Guid> Handle(CreateTechSupportAccountCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate that the managing Provider exists and is currently verified
        var provider = await context.Providers
            .FirstOrDefaultAsync(p => p.UserId == request.ProviderId, cancellationToken);

        if (provider == null)
        {
            throw new KeyNotFoundException($"Provider profile with ID '{request.ProviderId}' was not found.");
        }

        if (!provider.IsVerified)
        {
            throw new InvalidOperationException("Unverified providers are not permitted to provision technical support accounts.");
        }

        // 2. Delegate secure identity credential generation to the Infrastructure Identity Service
        // FIX: Explicitly deconstructing all 3 elements returned by your service tuple
        var (succeeded, identityUserId, errors) = await identityService.CreateUserAsync(
            request.Email,
            request.Password,
            "TechSupport");

        if (!succeeded)
        {
            throw new InvalidOperationException($"Failed to provision identity credentials: {string.Join(", ", errors)}");
        }

        // 3. Instantiate our domain transactional tracking entity mapped to the generated Identity User ID
        var techAccount = new TechSupportAccount(
            request.ProviderId,
            identityUserId,
            request.FullName);

        // 4. Record the tracking entity into our PostgreSQL persistence store
        context.TechSupportAccounts.Add(techAccount);
        await context.SaveChangesAsync(cancellationToken);

        // 5. Return the tracking key for frontend consumer verification
        return techAccount.Id;
    }
}
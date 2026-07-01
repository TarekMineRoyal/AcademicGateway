using AcademicGateway.Application.Common.Interfaces;
using Domain.Providers;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Providers.Commands.RegisterProvider;

/// <summary>
/// Handles the transaction routine to process a <see cref="RegisterProviderCommand"/>.
/// Provisions centralized corporate identity user credentials and instantiates a pending Provider aggregate root.
/// </summary>
public class RegisterProviderCommandHandler(
    IIdentityService identityService,
    IApplicationDbContext dbContext)
    : IRequestHandler<RegisterProviderCommand, Guid>
{
    /// <summary>
    /// Processes identity profile configuration, validates invariants, instantiates the corporate provider aggregate, and flushes states.
    /// </summary>
    /// <param name="request">The structural parameter bundle tracking requested credentials and registration specifications.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>The newly assigned tracking Guid identifying the materialized Provider aggregate root entity.</returns>
    /// <exception cref="InvalidOperationException">Thrown if identity profile creation fails via infrastructure layer restrictions.</exception>
    public async Task<Guid> Handle(RegisterProviderCommand request, CancellationToken cancellationToken)
    {
        // 1. Create the secure baseline application identity context user
        var (succeeded, userId, errors) = await identityService.CreateUserAsync(
            request.Username,
            request.Email,
            request.Password);

        if (!succeeded)
        {
            throw new InvalidOperationException($"Provider identity configuration failed: {string.Join(", ", errors)}");
        }

        // 2. Enforce Domain Encapsulation - Initialize via explicit parameterized constructor logic.
        // This guarantees all aggregate validation invariants fire cleanly prior to persistence layer mapping.
        var providerProfile = new Provider(
            id: userId,
            companyName: request.CompanyName
        );

        // 3. Invoke explicit domain aggregate behaviors to mutate tracking states securely.
        // This keeps initialization logic inside the domain clean and deterministic.
        providerProfile.UpdateProfileDetails(
            request.CompanyDescription,
            request.WebsiteUrl ?? string.Empty
        );

        dbContext.Providers.Add(providerProfile);

        // 4. Save to database within the single atomic transaction unit of work
        await dbContext.SaveChangesAsync(cancellationToken);

        return userId;
    }
}
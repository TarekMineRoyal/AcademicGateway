using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.Entities;
using MediatR;

namespace AcademicGateway.Application.Features.Users.Commands.RegisterProvider;

public class RegisterProviderCommandHandler(
    IIdentityService identityService,
    IApplicationDbContext dbContext)
    : IRequestHandler<RegisterProviderCommand, string>
{
    public async Task<string> Handle(RegisterProviderCommand request, CancellationToken cancellationToken)
    {
        // 1. Create the base Identity User
        var (succeeded, userId, errors) = await identityService.CreateUserAsync(
            request.Username,
            request.Email,
            request.Password);

        if (!succeeded)
        {
            throw new Exception($"Provider creation failed: {string.Join(", ", errors)}");
        }

        // 2. Create the Domain Profile
        var providerProfile = new Provider
        {
            UserId = userId,
            OrganizationName = request.OrganizationName,
            Industry = request.Industry,
            WebsiteUrl = request.WebsiteUrl,
            IsVerified = false // Enforcing the business rule that new providers start unverified
        };

        dbContext.Providers.Add(providerProfile);

        // 3. Save to database
        await dbContext.SaveChangesAsync(cancellationToken);

        return userId;
    }
}
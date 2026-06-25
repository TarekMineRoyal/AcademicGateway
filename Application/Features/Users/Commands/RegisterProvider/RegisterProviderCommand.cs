using MediatR;

namespace AcademicGateway.Application.Features.Users.Commands.RegisterProvider;

// The DTO specifically tailored for Provider registration
public record RegisterProviderCommand : IRequest<string>
{
    // Base Identity Properties
    public string Email { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;

    // Provider Specific Properties
    public string OrganizationName { get; init; } = string.Empty;
    public string Industry { get; init; } = string.Empty;
    public string? WebsiteUrl { get; init; }
}
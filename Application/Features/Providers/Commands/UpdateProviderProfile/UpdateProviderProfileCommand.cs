using MediatR;

namespace AcademicGateway.Application.Features.Providers.Commands.UpdateProviderProfile;

/// <summary>
/// CQRS Command to update an existing corporate provider's public-facing organization profile.
/// Enforces secure, contextual maintenance pathways for company details and branding properties.
/// </summary>
public record UpdateProviderProfileCommand : IRequest
{
    /// <summary>
    /// Gets the updated public-facing legal or operational corporate name of the provider organization.
    /// </summary>
    public string CompanyName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the updated detailed summary describing the firm's core operations, industry focus, and domains.
    /// </summary>
    public string CompanyDescription { get; init; } = string.Empty;

    /// <summary>
    /// Gets the updated official website corporate URL reference string used for institutional validation.
    /// </summary>
    public string WebsiteUrl { get; init; } = string.Empty;
}
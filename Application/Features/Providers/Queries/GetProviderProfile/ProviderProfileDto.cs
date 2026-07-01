using System;

namespace AcademicGateway.Application.Features.Providers.Queries.GetProviderProfile;

/// <summary>
/// Data Transfer Object representing the administrative and compliance profile details of a corporate Industry Provider.
/// </summary>
public record ProviderProfileDto
{
    /// <summary>
    /// Gets the global unique entity identifier assigned to this provider profile, mapping 1:1 to their security credentials.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the registered corporate name of the partner organization.
    /// </summary>
    public string CompanyName { get; init; } = string.Empty;

    /// <summary>
    /// Gets a detailed summary of the company's domain, industry focus, and core business operations.
    /// </summary>
    public string CompanyDescription { get; init; } = string.Empty;

    /// <summary>
    /// Gets the optional official web address or digital landing portal for the corporate entity.
    /// </summary>
    public string? WebsiteUrl { get; init; }

    /// <summary>
    /// Gets a value indicating whether this industry provider has successfully completed platform verification onboarding gates.
    /// </summary>
    public bool IsVerified { get; init; }
}
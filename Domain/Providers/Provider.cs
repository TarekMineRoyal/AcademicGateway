using System;
using System.Collections.Generic;
using System.Linq;
using Domain.ProjectTemplates;

namespace Domain.Providers;

/// <summary>
/// Represents an external organization, industry partner, or corporate provider 
/// capable of proposing project templates and sponsoring student opportunities.
/// </summary>
public class Provider
{
    private readonly List<ProjectTemplate> _projectTemplates = new();
    private readonly List<ProviderApplication> _providerApplications = new();

    /// <summary>
    /// Gets the unique identifier for the Provider profile. 
    /// Maps 1:1 to the underlying Identity ApplicationUser identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the registered corporate name of the provider organization.
    /// </summary>
    public string CompanyName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a detailed description of the company's domain, industry focus, and core operations.
    /// </summary>
    public string CompanyDescription { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the official website URL of the organization for student verification and visibility.
    /// </summary>
    public string WebsiteUrl { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether this provider has been formally verified and approved 
    /// through the onboarding funnel. Unverified providers cannot publish templates to students.
    /// </summary>
    public bool IsVerified { get; private set; }

    /// <summary>
    /// Gets the read-only tracking collection of project blueprints proposed by this organization.
    /// </summary>
    public IReadOnlyCollection<ProjectTemplate> ProjectTemplates => _projectTemplates.AsReadOnly();

    /// <summary>
    /// Gets the read-only history collection of onboarding applications submitted by this provider.
    /// </summary>
    public IReadOnlyCollection<ProviderApplication> ProviderApplications => _providerApplications.AsReadOnly();

    /// <summary>
    /// EF Core constructor requirement. Prevents bypass of domain constraints during persistence hydration.
    /// </summary>
    private Provider()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Provider"/> profile with default unverified standing.
    /// </summary>
    /// <param name="id">The unique Identity key linking back to the account credentials.</param>
    /// <param name="companyName">The legal or operational name of the company.</param>
    /// <exception cref="ArgumentException">Thrown when fundamental identity or name constraints are missing.</exception>
    public Provider(Guid id, string companyName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Identity User ID cannot be empty.", nameof(id));
        }

        Id = id;
        UpdateCompanyName(companyName);
        IsVerified = false; // Must pass a ProviderApplication assessment loop to become verified
    }

    /// <summary>
    /// Updates the public-facing corporate name of the provider.
    /// </summary>
    /// <param name="newCompanyName">The target company name.</param>
    /// <exception cref="ArgumentException">Thrown if the provided name value is blank.</exception>
    public void UpdateCompanyName(string newCompanyName)
    {
        if (string.IsNullOrWhiteSpace(newCompanyName))
        {
            throw new ArgumentException("Company name cannot be empty or whitespace.", nameof(newCompanyName));
        }

        CompanyName = newCompanyName.Trim();
    }

    /// <summary>
    /// Updates contextual and discoverable business details for the provider profile.
    /// </summary>
    /// <param name="description">The descriptive summary of the firm's focus fields.</param>
    /// <param name="websiteUrl">The verified corporate URL reference string.</param>
    public void UpdateProfileDetails(string description, string websiteUrl)
    {
        CompanyDescription = description?.Trim() ?? string.Empty;
        WebsiteUrl = websiteUrl?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Formally authorizes the provider, elevating their permission status to allow active template creation.
    /// Executed automatically when an associated onboarding application transitions to an Approved status.
    /// </summary>
    public void VerifyProfile()
    {
        IsVerified = true;
    }

    /// <summary>
    /// Revokes verified standing, freezing the provider's ability to issue new project blueprints.
    /// </summary>
    public void RevokeVerification()
    {
        IsVerified = false;
    }

    /// <summary>
    /// Securely tracks a newly initiated or processed verification application.
    /// </summary>
    /// <param name="application">The onboarding application tracking instance.</param>
    public void LinkApplication(ProviderApplication application)
    {
        ArgumentNullException.ThrowIfNull(application);

        if (_providerApplications.Any(a => a.Id == application.Id))
        {
            return;
        }

        _providerApplications.Add(application);
    }
}
using MediatR;
using System;

namespace AcademicGateway.Application.Features.Providers.Commands.RegisterProvider;

/// <summary>
/// CQRS Command to register a new corporate Industry Provider profile within the academic gateway.
/// Provisions baseline user identity credentials and prepares a pending Provider domain aggregate model context.
/// </summary>
public record RegisterProviderCommand : IRequest<Guid>
{
    /// <summary>
    /// Gets the corporate or institutional email address tracking the identity credential.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets the unique security username requested for authentication workflows.
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// Gets the plain-text password requested for credential configuration.
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Gets the registered corporate name of the partner organization.
    /// </summary>
    public string CompanyName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the descriptive operational background, capability statements, and industry focus fields of the partner company.
    /// Replaces the legacy, anemic primitive industry text fields.
    /// </summary>
    public string CompanyDescription { get; init; } = string.Empty;

    /// <summary>
    /// Gets the optional official web address or digital landing portal for the corporate entity.
    /// </summary>
    public string? WebsiteUrl { get; init; }
}
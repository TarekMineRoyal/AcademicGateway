using AcademicGateway.Domain.Common.Exceptions;
using System;

namespace AcademicGateway.Domain.Providers.Exceptions;

/// <summary>
/// Exception thrown when an unverified provider attempts to create or publish a project template proposal blueprint.
/// </summary>
public class ProviderNotVerifiedException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderNotVerifiedException"/> class.
    /// </summary>
    /// <param name="providerId">The unique identifier of the unverified provider.</param>
    public ProviderNotVerifiedException(Guid providerId)
        : base($"Provider profile '{providerId}' is not verified. Unverified providers are restricted from creating project templates.", "PROVIDER_NOT_VERIFIED")
    {
    }
}
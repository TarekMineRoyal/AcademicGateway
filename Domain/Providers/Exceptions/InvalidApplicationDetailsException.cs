using AcademicGateway.Domain.Common.Exceptions;

namespace AcademicGateway.Domain.Providers.Exceptions;

/// <summary>
/// Exception thrown when structural attributes, documentation links, or review metadata timestamps 
/// inside a <see cref="ProviderApplication"/> violate domain validation thresholds.
/// </summary>
public class InvalidApplicationDetailsException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidApplicationDetailsException"/> class.
    /// </summary>
    /// <param name="message">The human-readable explanation of the validation failure.</param>
    public InvalidApplicationDetailsException(string message)
        : base(message, "INVALID_APPLICATION_DETAILS")
    {
    }
}
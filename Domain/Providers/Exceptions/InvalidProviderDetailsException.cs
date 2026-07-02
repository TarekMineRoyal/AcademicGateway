using AcademicGateway.Domain.Common.Exceptions;

namespace AcademicGateway.Domain.Providers.Exceptions;

/// <summary>
/// Exception thrown when structural configuration details of a <see cref="Provider"/> 
/// (such as User IDs or company profile names) violate core business validation formatting.
/// </summary>
public class InvalidProviderDetailsException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidProviderDetailsException"/> class.
    /// </summary>
    /// <param name="message">The human-readable explanation of the validation failure.</param>
    public InvalidProviderDetailsException(string message)
        : base(message, "INVALID_PROVIDER_DETAILS")
    {
    }
}
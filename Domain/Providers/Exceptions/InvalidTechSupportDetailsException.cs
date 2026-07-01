using Domain.Common.Exceptions;

namespace Domain.Providers.Exceptions;

/// <summary>
/// Exception thrown when essential tracking specifications of a <see cref="TechSupportAccount"/> 
/// (such as User IDs, employee staff numbers, or access tiers) fail format criteria bounds.
/// </summary>
public class InvalidTechSupportDetailsException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidTechSupportDetailsException"/> class.
    /// </summary>
    /// <param name="message">The human-readable explanation of the validation failure.</param>
    public InvalidTechSupportDetailsException(string message)
        : base(message, "INVALID_TECH_SUPPORT_DETAILS")
    {
    }
}
using System;

namespace AcademicGateway.Domain.Common.Exceptions;

/// <summary>
/// Serves as the fundamental base class for all domain-specific business rule exceptions.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Gets the unique, machine-readable string token identifier for the specific error scenario 
    /// (e.g., "PROFESSOR_CAPACITY_REACHED").
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class.
    /// </summary>
    /// <param name="message">The human-readable explanation of the validation failure.</param>
    /// <param name="errorCode">The machine-readable tracking code for consumer clients.</param>
    protected DomainException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
using AcademicGateway.Domain.Common.Exceptions;

namespace AcademicGateway.Domain.Professors.Exceptions;

/// <summary>
/// Exception thrown when an assignment or modification of a professor's maximum supervision threshold 
/// falls below zero or drops lower than their current active project payload.
/// </summary>
public class InvalidSupervisionCapacityException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidSupervisionCapacityException"/> class.
    /// </summary>
    /// <param name="message">The human-readable explanation of the capacity discrepancy.</param>
    public InvalidSupervisionCapacityException(string message)
        : base(message, "INVALID_SUPERVISION_CAPACITY")
    {
    }
}
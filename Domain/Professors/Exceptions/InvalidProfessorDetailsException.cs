using AcademicGateway.Domain.Common.Exceptions;

namespace AcademicGateway.Domain.Professors.Exceptions;

/// <summary>
/// Exception thrown when structural profile fields of a <see cref="Professor"/> 
/// (such as ID, name, department, or rank) fail baseline string formatting and identification invariants.
/// </summary>
public class InvalidProfessorDetailsException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidProfessorDetailsException"/> class.
    /// </summary>
    /// <param name="message">The human-readable explanation of the validation failure.</param>
    public InvalidProfessorDetailsException(string message)
        : base(message, "INVALID_PROFESSOR_DETAILS")
    {
    }
}
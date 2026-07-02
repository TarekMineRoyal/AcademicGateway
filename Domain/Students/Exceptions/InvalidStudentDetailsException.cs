using AcademicGateway.Domain.Common.Exceptions;

namespace AcademicGateway.Domain.Students.Exceptions;

/// <summary>
/// Exception thrown when structural profile configurations of a <see cref="Student"/> 
/// (such as User IDs, names, or malformed join entity routing keys) fail business criteria constraints.
/// </summary>
public class InvalidStudentDetailsException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidStudentDetailsException"/> class.
    /// </summary>
    /// <param name="message">The human-readable explanation of the validation failure.</param>
    public InvalidStudentDetailsException(string message)
        : base(message, "INVALID_STUDENT_DETAILS")
    {
    }
}
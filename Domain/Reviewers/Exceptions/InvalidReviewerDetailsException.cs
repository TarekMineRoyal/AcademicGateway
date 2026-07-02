using AcademicGateway.Domain.Common.Exceptions;
using AcademicGateway.Domain.Reviewers;

namespace AcademicGateway.Domain.Reviewers.Exceptions;

/// <summary>
/// Exception thrown when structural profile configurations of a <see cref="Reviewer"/> 
/// (such as User IDs or full name definitions) fail basic validation or format criteria.
/// </summary>
public class InvalidReviewerDetailsException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidReviewerDetailsException"/> class.
    /// </summary>
    /// <param name="message">The human-readable explanation of the validation failure.</param>
    public InvalidReviewerDetailsException(string message)
        : base(message, "INVALID_REVIEWER_DETAILS")
    {
    }
}
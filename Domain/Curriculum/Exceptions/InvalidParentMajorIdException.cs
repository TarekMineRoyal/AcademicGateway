using AcademicGateway.Domain.Common.Exceptions;

namespace AcademicGateway.Domain.Curriculum.Exceptions;

/// <summary>
/// Exception thrown when a child <see cref="Specialty"/> is structurally initialized 
/// with an empty or default <see cref="System.Guid"/> identifier for its parent Major entity context.
/// </summary>
public class InvalidParentMajorIdException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidParentMajorIdException"/> class 
    /// with an explicit message and unique machine-readable error token.
    /// </summary>
    public InvalidParentMajorIdException()
        : base("Specialty must be linked to a valid parent major ID.", "INVALID_PARENT_MAJOR_ID")
    {
    }
}
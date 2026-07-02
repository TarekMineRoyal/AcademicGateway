using AcademicGateway.Domain.Common.Exceptions;

namespace AcademicGateway.Domain.Curriculum.Exceptions;

/// <summary>
/// Exception thrown when an attempt is made to initialize or mutate a <see cref="Major"/> 
/// using an empty, null, or whitespace text title value.
/// </summary>
public class EmptyMajorNameException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmptyMajorNameException"/> class 
    /// with an explicit message and unique machine-readable error token.
    /// </summary>
    public EmptyMajorNameException()
        : base("Major name cannot be empty or whitespace.", "EMPTY_MAJOR_NAME")
    {
    }
}
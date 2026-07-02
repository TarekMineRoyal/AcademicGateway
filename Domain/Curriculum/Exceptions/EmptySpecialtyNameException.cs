using AcademicGateway.Domain.Common.Exceptions;

namespace AcademicGateway.Domain.Curriculum.Exceptions;

/// <summary>
/// Exception thrown when an attempt is made to initialize or mutate a child <see cref="Specialty"/> 
/// track using an empty, null, or whitespace text title value.
/// </summary>
public class EmptySpecialtyNameException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmptySpecialtyNameException"/> class 
    /// with an explicit message and unique machine-readable error token.
    /// </summary>
    public EmptySpecialtyNameException()
        : base("Specialty name cannot be empty or whitespace.", "EMPTY_SPECIALTY_NAME")
    {
    }
}
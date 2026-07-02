using AcademicGateway.Domain.Common.Exceptions;

namespace AcademicGateway.Domain.Students.Exceptions;

/// <summary>
/// Exception thrown when a student's declared or expected graduation completion year 
/// is assigned an impossible historical date prior to the platform benchmark threshold (Year 2000).
/// </summary>
public class InvalidGraduationYearException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidGraduationYearException"/> class.
    /// </summary>
    /// <param name="invalidYear">The malformed graduation year that caused the exception.</param>
    public InvalidGraduationYearException(int invalidYear)
        : base($"The expected graduation year '{invalidYear}' is invalid. Dates cannot precede the year 2000 threshold.", "INVALID_GRADUATION_YEAR")
    {
    }
}
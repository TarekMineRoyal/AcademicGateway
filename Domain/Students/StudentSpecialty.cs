using System;
using Domain.Curriculum;
using Domain.Students.Exceptions;

namespace Domain.Students;

/// <summary>
/// Represents the explicit many-to-many join entity linking a Student profile to a specific academic Specialty.
/// </summary>
public class StudentSpecialty
{
    /// <summary>
    /// Gets the foreign key identifier for the associated student profile (corresponds to the unique Identity User ID).
    /// </summary>
    public Guid StudentId { get; private set; }

    /// <summary>
    /// Gets the navigation property for the associated student profile.
    /// </summary>
    public Student Student { get; private set; } = null!;

    /// <summary>
    /// Gets the foreign key identifier for the associated sub-specialty.
    /// </summary>
    public Guid SpecialtyId { get; private set; }

    /// <summary>
    /// Gets the navigation property for the associated sub-specialty.
    /// </summary>
    public Specialty Specialty { get; private set; } = null!;

    /// <summary>
    /// EF Core constructor requirement. Prevents bypass of standard initialization during database materialization.
    /// </summary>
    private StudentSpecialty()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StudentSpecialty"/> join entity with required tracking identifiers.
    /// </summary>
    /// <param name="studentId">The unique Identity string identifier of the target student profile.</param>
    /// <param name="specialtyId">The unique identifier of the target sub-specialty.</param>
    /// <exception cref="InvalidStudentDetailsException">Thrown when studentId or specialtyId is an empty Guid.</exception>
    public StudentSpecialty(Guid studentId, Guid specialtyId)
    {
        if (studentId == Guid.Empty)
        {
            throw new InvalidStudentDetailsException("Student tracking identity parameters cannot be empty mapping references.");
        }

        if (specialtyId == Guid.Empty)
        {
            throw new InvalidStudentDetailsException("Sub-track specialty tracking identity parameters cannot be empty mapping references.");
        }

        StudentId = studentId;
        SpecialtyId = specialtyId;
    }
}
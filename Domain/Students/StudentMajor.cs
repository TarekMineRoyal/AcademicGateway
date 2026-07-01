using System;
using Domain.Curriculum;
using Domain.Students.Exceptions;

namespace Domain.Students;

/// <summary>
/// Represents the explicit many-to-many join entity linking a Student profile to an academic Major.
/// </summary>
public class StudentMajor
{
    /// <summary>
    /// Gets the foreign key identifier for the associated student.
    /// </summary>
    public Guid StudentId { get; private set; }

    /// <summary>
    /// Gets the navigation property for the associated student profile.
    /// </summary>
    public Student Student { get; private set; } = null!;

    /// <summary>
    /// Gets the foreign key identifier for the associated academic major.
    /// </summary>
    public Guid MajorId { get; private set; }

    /// <summary>
    /// Gets the navigation property for the associated academic major.
    /// </summary>
    public Major Major { get; private set; } = null!;

    /// <summary>
    /// EF Core constructor requirement. Prevents bypass of standard initialization during materialization.
    /// </summary>
    private StudentMajor()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StudentMajor"/> join entity with required tracking identifiers.
    /// </summary>
    /// <param name="studentId">The unique identifier of the target student profile.</param>
    /// <param name="majorId">The unique identifier of the target academic major.</param>
    /// <exception cref="InvalidStudentDetailsException">Thrown when either studentId or majorId is an empty Guid.</exception>
    public StudentMajor(Guid studentId, Guid majorId)
    {
        if (studentId == Guid.Empty)
        {
            throw new InvalidStudentDetailsException("Student tracking identity parameters cannot be empty mapping references.");
        }

        if (majorId == Guid.Empty)
        {
            throw new InvalidStudentDetailsException("Academic major tracking identity parameters cannot be empty mapping references.");
        }

        StudentId = studentId;
        MajorId = majorId;
    }
}
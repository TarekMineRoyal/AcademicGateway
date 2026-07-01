using System;
using System.Collections.Generic;
using Domain.Students;

namespace Domain.Curriculum;

/// <summary>
/// Represents an academic sub-specialty nested under a specific major (e.g., Software Engineering under Computer Science).
/// </summary>
public class Specialty
{
    private readonly List<StudentSpecialty> _studentSpecialties = new();

    /// <summary>
    /// Gets the unique identifier for the Specialty.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the descriptive name of the sub-specialty.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the foreign key identifier mapping this specialty to its parent academic major.
    /// </summary>
    public Guid MajorId { get; private set; }

    /// <summary>
    /// Gets the navigation property for the parent academic major.
    /// </summary>
    public Major Major { get; private set; } = null!;

    /// <summary>
    /// Gets the read-only tracking collection of student profiles specialized under this track.
    /// </summary>
    public IReadOnlyCollection<StudentSpecialty> StudentSpecialties => _studentSpecialties.AsReadOnly();

    /// <summary>
    /// EF Core constructor requirement. Prevents bypass of business validations during standard initialization.
    /// </summary>
    private Specialty()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Specialty"/> class with required domain validations.
    /// </summary>
    /// <param name="name">The name of the sub-specialty.</param>
    /// <param name="majorId">The unique identifier of the parent major this specialty belongs to.</param>
    /// <exception cref="ArgumentException">Thrown when name is empty or majorId is an empty Guid.</exception>
    internal Specialty(string name, Guid majorId)
    {
        if (majorId == Guid.Empty)
        {
            throw new ArgumentException("Specialty must be linked to a valid parent major ID.", nameof(majorId));
        }

        UpdateName(name);
        MajorId = majorId;
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Updates the descriptive name of the specialty, verifying business rule boundaries.
    /// </summary>
    /// <param name="newName">The new name for the specialty.</param>
    /// <exception cref="ArgumentException">Thrown if the provided name is invalid.</exception>
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("Specialty name cannot be empty or whitespace.", nameof(newName));
        }

        Name = newName.Trim();
    }
}
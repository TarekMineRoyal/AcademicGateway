using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Common;
using Domain.Students;

namespace Domain.Curriculum;

/// <summary>
/// Represents an academic major within the gateway (e.g., Computer Science, Mechanical Engineering).
/// </summary>
public class Major : BaseEntity
{
    private readonly List<Specialty> _specialties = new();
    private readonly List<StudentMajor> _studentMajors = new();

    /// <summary>
    /// Gets the unique identifier for the Major.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the unique, descriptive name of the academic major.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the read-only collection of specialties associated with this major.
    /// </summary>
    public IReadOnlyCollection<Specialty> Specialties => _specialties.AsReadOnly();

    /// <summary>
    /// Gets the read-only tracking collection of students assigned to this major.
    /// </summary>
    public IReadOnlyCollection<StudentMajor> StudentMajors => _studentMajors.AsReadOnly();

    /// <summary>
    /// EF Core constructor requirement. Prevents bypass of business validations during standard initialization.
    /// </summary>
    private Major()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Major"/> class with required business validations.
    /// </summary>
    /// <param name="name">The name of the academic major.</param>
    /// <exception cref="ArgumentException">Thrown when the name is null, empty, or whitespace.</exception>
    public Major(string name)
    {
        UpdateName(name);
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Updates the name of the major, verifying business rule boundaries.
    /// </summary>
    /// <param name="newName">The new name for the major.</param>
    /// <exception cref="ArgumentException">Thrown if the provided name is invalid.</exception>
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("Major name cannot be empty or whitespace.", nameof(newName));
        }

        Name = newName.Trim();
    }

    /// <summary>
    /// Safely creates and adds a new sub-specialty under this academic major while avoiding duplicate entries.
    /// </summary>
    /// <param name="name">The descriptive name of the sub-specialty to add.</param>
    /// <exception cref="ArgumentException">Thrown when the provided name is null, empty, or whitespace.</exception>
    public void AddSpecialty(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Specialty name cannot be empty or whitespace.", nameof(name));
        }

        if (_specialties.Any(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            return; // Specialty already exists under this major, ignore to prevent duplication
        }

        // The aggregate root now explicitly controls the instantiation, 
        // automatically binding the specialty to this Major's unique Id.
        _specialties.Add(new Specialty(name, Id));
    }

    /// <summary>
    /// Safely removes an existing sub-specialty from this academic major.
    /// </summary>
    /// <param name="specialtyId">The unique identifier of the specialty to remove.</param>
    public void RemoveSpecialty(Guid specialtyId)
    {
        var specialty = _specialties.FirstOrDefault(s => s.Id == specialtyId);
        if (specialty != null)
        {
            _specialties.Remove(specialty);
        }
    }
}
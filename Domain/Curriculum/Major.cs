using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Common;
using Domain.Curriculum.Exceptions;
using Domain.Students;

namespace Domain.Curriculum;

/// <summary>
/// Represents a Major aggregate root within the academic gateway curriculum subsystem.
/// Orchestrates validation constraints and lifecycle adjustments for underlying sub-specialties.
/// </summary>
public class Major : BaseEntity
{
    private readonly List<Specialty> _specialties = new();
    private readonly List<StudentMajor> _studentMajors = new();

    /// <summary>
    /// Gets the unique surrogate tracking identification primary key for this Major.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the standardized system name description given to this academic major track.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a read-only encapsulated collection of sub-specialty paths nested under this Major aggregate context.
    /// </summary>
    public IReadOnlyCollection<Specialty> Specialties => _specialties.AsReadOnly();

    /// <summary>
    /// Gets a read-only tracking connection mapping students allocated to this Major.
    /// </summary>
    public IReadOnlyCollection<StudentMajor> StudentMajors => _studentMajors.AsReadOnly();

    /// <summary>
    /// Required parameterless constructor variant for Entity Framework Core relational database hydration mappings.
    /// </summary>
    private Major()
    {
    }

    /// <summary>
    /// Initializes a new valid domain instance of the <see cref="Major"/> aggregate root.
    /// </summary>
    /// <param name="name">The intended text title identifier for the academic major.</param>
    /// <exception cref="EmptyMajorNameException">Thrown when the targeted title value evaluation fails state requirements.</exception>
    public Major(string name)
    {
        UpdateName(name);
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Mutates the title value of this Major aggregate root after validating baseline format invariants.
    /// </summary>
    /// <param name="newName">The target name value string to enforce.</param>
    /// <exception cref="EmptyMajorNameException">Thrown when the string argument provides null or whitespace parameters.</exception>
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new EmptyMajorNameException();
        }

        Name = newName.Trim();
    }

    /// <summary>
    /// Instantiates and nests a new child track specialty item under this Major aggregate tracking instance.
    /// Defensively guards internal collections from experiencing matching textual duplicates.
    /// </summary>
    /// <param name="name">The descriptive path title assigned to the new specialty sub-concentration.</param>
    /// <exception cref="EmptySpecialtyNameException">Thrown when the name parameter contains zero content or invalid characters.</exception>
    public void AddSpecialty(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new EmptySpecialtyNameException();
        }

        // Domain Invariant Checklist: Block execution if a tracking child concentration already claims this name value.
        if (_specialties.Any(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        _specialties.Add(new Specialty(name, Id));
    }

    /// <summary>
    /// Purges a child specialty path tracking context out of this Major aggregate boundary reference.
    /// </summary>
    /// <param name="specialtyId">The unique surrogate key identifying the child specialty candidate targeted for deletion.</param>
    public void RemoveSpecialty(Guid specialtyId)
    {
        var specialty = _specialties.FirstOrDefault(s => s.Id == specialtyId);
        if (specialty != null)
        {
            _specialties.Remove(specialty);
        }
    }
}
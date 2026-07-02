using System;
using System.Collections.Generic;
using AcademicGateway.Domain.Curriculum.Exceptions;
using AcademicGateway.Domain.Students;

namespace AcademicGateway.Domain.Curriculum;

/// <summary>
/// Represents a specific academic sub-track concentration context linked strictly to a primary parent <see cref="Major"/>.
/// Acts strictly as an dependent child tracking item boundary inside the Curriculum aggregate workspace design.
/// </summary>
public class Specialty
{
    private readonly List<StudentSpecialty> _studentSpecialties = new();

    /// <summary>
    /// Gets the unique primary tracking identity surrogate key for this child Specialty track item.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the text concentration path title name given to this specialty sub-track (e.g., "Cybersecurity").
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the invariant foreign structural routing tracker reference identifier key toward the parent Major aggregate root.
    /// </summary>
    public Guid MajorId { get; private set; }

    /// <summary>
    /// Gets or sets the formal domain navigation relationship asset instance pointing back towards the parent Major root entity framework.
    /// </summary>
    public Major Major { get; private set; } = null!;

    /// <summary>
    /// Gets a read-only tracking link collection of students assigned under this specific concentration pathway track.
    /// </summary>
    public IReadOnlyCollection<StudentSpecialty> StudentSpecialties => _studentSpecialties.AsReadOnly();

    /// <summary>
    /// Required parameterless constructor variant for Entity Framework Core relational database hydration mappings.
    /// </summary>
    private Specialty()
    {
    }

    /// <summary>
    /// Initializes a new dependent instance child element configuration for a <see cref="Specialty"/> track.
    /// Restricted explicitly to internal assembly scopes to enforce creation routing strictly through parent aggregate method vectors.
    /// </summary>
    /// <param name="name">The text title value context allocated to the new specialty tracking item.</param>
    /// <param name="majorId">The concrete tracking key matching the root host Major identity location parameter.</param>
    /// <exception cref="InvalidParentMajorIdException">Thrown if the provided parent major tracking key evaluation returns empty boundaries.</exception>
    /// <exception cref="EmptySpecialtyNameException">Thrown if the given text name fails validation constraints.</exception>
    internal Specialty(string name, Guid majorId)
    {
        if (majorId == Guid.Empty)
        {
            throw new InvalidParentMajorIdException();
        }

        UpdateName(name);
        MajorId = majorId;
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Mutates the title definition string parameters tracking this dependent child track item state values.
    /// </summary>
    /// <param name="newName">The new customized name string to map onto the entity fields.</param>
    /// <exception cref="EmptySpecialtyNameException">Thrown when input arguments resolve to blank formatting values.</exception>
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new EmptySpecialtyNameException();
        }

        Name = newName.Trim();
    }
}
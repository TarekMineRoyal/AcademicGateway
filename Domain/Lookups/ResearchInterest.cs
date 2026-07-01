using System;
using System.Collections.Generic;
using Domain.Professors;

namespace Domain.Lookups;

/// <summary>
/// Represents a specialized academic or scientific research area (e.g., Computer Vision, Quantum Computing, Behavioral Economics).
/// </summary>
public class ResearchInterest
{
    private readonly List<ProfessorResearchInterest> _professorResearchInterests = new();

    /// <summary>
    /// Gets the unique identifier for the research interest.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the unique, descriptive title of the research domain.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the read-only tracking collection of professors specialized in this domain.
    /// </summary>
    public IReadOnlyCollection<ProfessorResearchInterest> ProfessorResearchInterests => _professorResearchInterests.AsReadOnly();

    /// <summary>
    /// EF Core constructor requirement. Prevents bypass of domain constraints during hydration.
    /// </summary>
    private ResearchInterest()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResearchInterest"/> class with domain validations.
    /// </summary>
    /// <param name="name">The name or description of the research specialty.</param>
    /// <exception cref="ArgumentException">Thrown when name criteria checks fail.</exception>
    public ResearchInterest(string name)
    {
        UpdateName(name);
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Updates the descriptive name of the research focus area.
    /// </summary>
    /// <param name="newName">The target name value.</param>
    /// <exception cref="ArgumentException">Thrown if the provided name is invalid.</exception>
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("Research interest name cannot be empty or whitespace.", nameof(newName));
        }

        Name = newName.Trim();
    }
}
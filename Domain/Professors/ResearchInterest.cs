using System;
using System.Collections.Generic;
using AcademicGateway.Domain.Common;
using AcademicGateway.Domain.Professors.Exceptions;

namespace AcademicGateway.Domain.Professors;

/// <summary>
/// Represents a specific global academic Research Interest topic categorizer lookup index.
/// Governs structural integrity values assigned onto lookup categories mapped across faculty members.
/// </summary>
public class ResearchInterest : BaseEntity
{
    private readonly List<ProfessorResearchInterest> _professorLinks = new();

    /// <summary>
    /// Gets the unique tracking primary surrogate key assigned to this Research Interest metadata definition.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the contextual textual area name descriptive title mapping for this category (e.g., "Machine Learning").
    /// </summary>
    public string Area { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a read-only tracking relationship collection linking backing professors associated to this topic focus index area.
    /// </summary>
    public IReadOnlyCollection<ProfessorResearchInterest> ProfessorLinks => _professorLinks.AsReadOnly();

    /// <summary>
    /// Required parameterless constructor variant for Entity Framework Core relational database hydration mappings.
    /// </summary>
    private ResearchInterest()
    {
    }

    /// <summary>
    /// Initializes a new valid domain instance of the <see cref="ResearchInterest"/> asset.
    /// </summary>
    /// <param name="area">The intended descriptive label text assigned onto this lookup track category.</param>
    /// <exception cref="EmptyResearchInterestAreaException">Thrown when input evaluations discover blank text arguments.</exception>
    public ResearchInterest(string area)
    {
        UpdateArea(area);
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Mutates the descriptive structural metadata text marking this focus category reference track tracking parameters.
    /// </summary>
    /// <param name="newArea">The updated text value expression string to save into the database row properties.</param>
    /// <exception cref="EmptyResearchInterestAreaException">Thrown when string parameters result in null or whitespace parameters.</exception>
    public void UpdateArea(string newArea)
    {
        if (string.IsNullOrWhiteSpace(newArea))
        {
            throw new EmptyResearchInterestAreaException();
        }

        Area = newArea.Trim();
    }
}
using System;
using Domain.Lookups;

namespace Domain.Professors;

/// <summary>
/// Represents the explicit many-to-many join entity linking a Professor profile to a specialized ResearchInterest lookup.
/// </summary>
public class ProfessorResearchInterest
{
    /// <summary>
    /// Gets the foreign key identifier for the associated professor profile.
    /// </summary>
    public string ProfessorId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the navigation property for the associated professor profile.
    /// </summary>
    public Professor Professor { get; private set; } = null!;

    /// <summary>
    /// Gets the foreign key identifier for the tracked research interest entity.
    /// </summary>
    public Guid ResearchInterestId { get; private set; }

    /// <summary>
    /// Gets the navigation property for the tracked research interest entity.
    /// </summary>
    public ResearchInterest ResearchInterest { get; private set; } = null!;

    /// <summary>
    /// EF Core constructor requirement.
    /// </summary>
    private ProfessorResearchInterest()
    {
    }

    /// <summary>
    /// Initializes a new instance of the many-to-many join tracking record.
    /// </summary>
    /// <param name="professorId">The string user identifier for the target faculty profile.</param>
    /// <param name="researchInterestId">The unique lookup key for the research domain.</param>
    /// <exception cref="ArgumentException">Thrown if input validation constraints fail.</exception>
    public ProfessorResearchInterest(string professorId, Guid researchInterestId)
    {
        if (string.IsNullOrWhiteSpace(professorId))
        {
            throw new ArgumentException("Professor User ID cannot be empty or whitespace.", nameof(professorId));
        }

        if (researchInterestId == Guid.Empty)
        {
            throw new ArgumentException("Research Interest ID cannot be an empty Guid.", nameof(researchInterestId));
        }

        ProfessorId = professorId;
        ResearchInterestId = researchInterestId;
    }
}
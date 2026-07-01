using System;
using Domain.Professors.Exceptions;

namespace Domain.Professors;

/// <summary>
/// Represents the explicit many-to-many join entity linking a <see cref="Professor"/> profile 
/// to a specialized academic or scientific <see cref="ResearchInterest"/> lookup indexing track.
/// Acts strictly as an immutable structural mapping entity inside the Professors aggregate boundary workspace.
/// </summary>
public class ProfessorResearchInterest
{
    /// <summary>
    /// Gets the unique foreign key identifier for the associated professor profile.
    /// </summary>
    public Guid ProfessorId { get; private set; }

    /// <summary>
    /// Gets the formal domain navigation relationship property pointing back to the associated parent professor aggregate root.
    /// </summary>
    public Professor Professor { get; private set; } = null!;

    /// <summary>
    /// Gets the unique foreign key identifier for the tracked research interest metadata definition.
    /// </summary>
    public Guid ResearchInterestId { get; private set; }

    /// <summary>
    /// Gets the formal domain navigation relationship property pointing to the associated research interest lookup entity.
    /// </summary>
    public ResearchInterest ResearchInterest { get; private set; } = null!;

    /// <summary>
    /// Required parameterless constructor variant for Entity Framework Core relational database hydration mappings.
    /// Prevents bypass of standard initialization constraints during database hydration.
    /// </summary>
    private ProfessorResearchInterest()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfessorResearchInterest"/> many-to-many join tracking record.
    /// </summary>
    /// <param name="professorId">The unique user identifier for the target faculty profile.</param>
    /// <param name="researchInterestId">The unique lookup key for the research domain.</param>
    /// <exception cref="InvalidProfessorDetailsException">Thrown when either tracking identifier is an empty Guid.</exception>
    public ProfessorResearchInterest(Guid professorId, Guid researchInterestId)
    {
        if (professorId == Guid.Empty)
        {
            throw new InvalidProfessorDetailsException("Professor identification tracker context cannot be empty.");
        }

        if (researchInterestId == Guid.Empty)
        {
            throw new InvalidProfessorDetailsException("Research interest identity tracking reference context cannot be empty.");
        }

        ProfessorId = professorId;
        ResearchInterestId = researchInterestId;
    }
}
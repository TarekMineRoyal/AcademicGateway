using Domain.Common.Exceptions;

namespace Domain.Professors.Exceptions;

/// <summary>
/// Exception thrown when an attempt is made to construct or modify a <see cref="ResearchInterest"/> 
/// using a null, empty, or whitespace textual description tracker.
/// </summary>
public class EmptyResearchInterestAreaException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmptyResearchInterestAreaException"/> class.
    /// </summary>
    public EmptyResearchInterestAreaException()
        : base("Research interest focus area cannot be empty or whitespace.", "EMPTY_RESEARCH_INTEREST_AREA")
    {
    }
}
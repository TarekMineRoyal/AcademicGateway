using AcademicGateway.Domain.Common.Exceptions;

namespace AcademicGateway.Domain.Skills.Exceptions;

/// <summary>
/// Exception thrown when an attempt is made to initialize or mutate a <see cref="Skill"/> 
/// using a null, empty, or whitespace textual name identifier.
/// </summary>
public class EmptySkillNameException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmptySkillNameException"/> class 
    /// with an explicit message and unique machine-readable error token.
    /// </summary>
    public EmptySkillNameException()
        : base("Skill name cannot be empty or whitespace.", "EMPTY_SKILL_NAME")
    {
    }
}
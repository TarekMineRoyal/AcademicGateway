using Domain.Common.Exceptions;

namespace Domain.ProjectTemplates.Exceptions;

/// <summary>
/// Exception thrown when structural attributes or data arrays of a <see cref="ProjectTemplate"/> 
/// (such as titles, summaries, empty skill mappings, or feedback logs) fail business format criteria.
/// </summary>
public class InvalidTemplateDetailsException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidTemplateDetailsException"/> class.
    /// </summary>
    /// <param name="message">The human-readable explanation of the validation failure.</param>
    public InvalidTemplateDetailsException(string message)
        : base(message, "INVALID_TEMPLATE_DETAILS")
    {
    }
}
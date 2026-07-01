using Domain.Common.Exceptions;
using Domain.ProjectTemplates.Enums;

namespace Domain.ProjectTemplates.Exceptions;

/// <summary>
/// Exception thrown when a lifecycle operation is attempted on a <see cref="ProjectTemplate"/> 
/// that is invalid given its current workflow state.
/// </summary>
public class InvalidTemplateStatusException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidTemplateStatusException"/> class.
    /// </summary>
    /// <param name="currentStatus">The current state of the project template aggregate.</param>
    /// <param name="attemptedAction">The descriptive name of the business method action being blocked.</param>
    public InvalidTemplateStatusException(ProjectTemplateStatus currentStatus, string attemptedAction)
        : base($"Cannot execute action '{attemptedAction}' while the project template is in the '{currentStatus}' state.", "INVALID_TEMPLATE_STATUS")
    {
    }
}
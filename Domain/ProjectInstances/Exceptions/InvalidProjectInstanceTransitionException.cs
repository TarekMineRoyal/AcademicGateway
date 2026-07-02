using AcademicGateway.Domain.Common.Exceptions;
using AcademicGateway.Domain.ProjectInstances.Enums;

namespace AcademicGateway.Domain.ProjectInstances.Exceptions;

/// <summary>
/// Thrown when an illegal lifecycle state transition is attempted on a project instance.
/// </summary>
public class InvalidProjectInstanceTransitionException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidProjectInstanceTransitionException"/> class.
    /// </summary>
    /// <param name="currentStatus">The current state of the project instance.</param>
    /// <param name="attemptedStatus">The illegal state transition that was attempted.</param>
    public InvalidProjectInstanceTransitionException(ProjectInstanceStatus currentStatus, ProjectInstanceStatus attemptedStatus)
        : base($"Cannot transition project instance from state '{currentStatus}' to '{attemptedStatus}'.", "PROJECT_INSTANCE_INVALID_TRANSITION")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidProjectInstanceTransitionException"/> class with a custom reason.
    /// </summary>
    /// <param name="message">The custom error message detailing the transition failure.</param>
    public InvalidProjectInstanceTransitionException(string message)
        : base(message, "PROJECT_INSTANCE_INVALID_TRANSITION")
    {
    }
}
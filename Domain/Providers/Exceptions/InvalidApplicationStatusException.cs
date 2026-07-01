using Domain.Common.Exceptions;
using Domain.Providers.Enums;

namespace Domain.Providers.Exceptions;

/// <summary>
/// Exception thrown when an onboarding workflow transition is executed on a <see cref="ProviderApplication"/> 
/// that is illegal given its current state machine position.
/// </summary>
public class InvalidApplicationStatusException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidApplicationStatusException"/> class.
    /// </summary>
    /// <param name="currentStatus">The current state of the provider application.</param>
    /// <param name="attemptedAction">The descriptive name of the blocked method action.</param>
    public InvalidApplicationStatusException(ProviderApplicationStatus currentStatus, string attemptedAction)
        : base($"Cannot execute action '{attemptedAction}' while the provider application is in the '{currentStatus}' state.", "INVALID_APPLICATION_STATUS")
    {
    }
}
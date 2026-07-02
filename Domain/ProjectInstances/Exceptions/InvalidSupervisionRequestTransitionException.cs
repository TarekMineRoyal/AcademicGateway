using AcademicGateway.Domain.Common.Exceptions;
using AcademicGateway.Domain.ProjectInstances.Enums;

namespace AcademicGateway.Domain.ProjectInstances.Exceptions;

/// <summary>
/// Thrown when a professor attempts to accept or reject an academic supervision request 
/// that is no longer in a pending state.
/// </summary>
public class InvalidSupervisionRequestTransitionException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidSupervisionRequestTransitionException"/> class.
    /// </summary>
    /// <param name="currentStatus">The current state of the supervision request.</param>
    public InvalidSupervisionRequestTransitionException(SupervisionRequestStatus currentStatus)
        : base($"Cannot alter supervision request. The request is currently '{currentStatus}' and must be 'Pending'.", "SUPERVISION_REQUEST_INVALID_TRANSITION")
    {
    }
}
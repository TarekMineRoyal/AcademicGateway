using AcademicGateway.Domain.Common.Exceptions;
using AcademicGateway.Domain.ProjectInstances.Enums;

namespace AcademicGateway.Domain.ProjectInstances.Exceptions;

/// <summary>
/// Thrown when a student attempts to evaluate a corporate technical support proposal 
/// that is not in an active pending review state.
/// </summary>
public class InvalidTechSupportProposalTransitionException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidTechSupportProposalTransitionException"/> class.
    /// </summary>
    /// <param name="currentStatus">The current state of the tech support proposal.</param>
    public InvalidTechSupportProposalTransitionException(TechSupportProposalStatus currentStatus)
        : base($"Cannot respond to corporate tech support proposal. The current status is '{currentStatus}', but it must be 'Pending'.", "TECH_SUPPORT_PROPOSAL_INVALID_TRANSITION")
    {
    }
}
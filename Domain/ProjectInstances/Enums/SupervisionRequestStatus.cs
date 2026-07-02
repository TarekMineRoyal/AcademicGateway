namespace AcademicGateway.Domain.ProjectInstances.Enums;

/// <summary>
/// Governs the workflow state of an academic match invitation sent from a student to a professor.
/// </summary>
public enum SupervisionRequestStatus
{
    /// <summary>
    /// The request has been submitted by the student with an attached pitch and is 
    /// awaiting evaluation from the chosen professor.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// The professor has approved the invitation, transitioning the parent project instance 
    /// into an Active status and granting the professor co-management permissions over milestones.
    /// </summary>
    Accepted = 2,

    /// <summary>
    /// The professor declined the invitation. This frees the student to either pivot the 
    /// project instance into a solo run or issue a new request to a different supervisor.
    /// </summary>
    Rejected = 3
}
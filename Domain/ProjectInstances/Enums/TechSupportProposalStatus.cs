namespace AcademicGateway.Domain.ProjectInstances.Enums;

/// <summary>
/// Tracks the state of a provider's suggestion to attach a corporate Tech Support account 
/// as an industry mentor to a specific running project instance.
/// </summary>
public enum TechSupportProposalStatus
{
    /// <summary>
    /// The provider has proposed a tech support account to assist with the instance, 
    /// and the proposal is awaiting the student's acceptance or rejection.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// The student has welcomed the technical mentor, officially granting the assigned 
    /// tech support user comment, view, and task tracking access on their project board.
    /// </summary>
    Accepted = 2,

    /// <summary>
    /// The student opted to decline the corporate mentorship proposal, leaving the project 
    /// workspace configuration unchanged.
    /// </summary>
    Rejected = 3
}
namespace AcademicGateway.Domain.ProjectInstances.Enums;

/// <summary>
/// Represents the definitive operational lifecycle states of a live project workspace.
/// </summary>
public enum ProjectInstanceStatus
{
    /// <summary>
    /// The project was initiated with a designated Academic Supervisor and is currently 
    /// paused/locked until the professor reviews and accepts the request.
    /// </summary>
    AwaitingSupervision = 1,

    /// <summary>
    /// The project is live and running. This state is reached immediately if started solo, 
    /// or transitioned into once a pending supervision request is accepted. 
    /// Milestones, tasks, and mentorship engagement are fully unlocked.
    /// </summary>
    Active = 2,

    /// <summary>
    /// The project has concluded its operational timeframe or reached its goals. 
    /// It is frozen and awaiting academic evaluation/grading if running under a supervisor.
    /// </summary>
    Concluded = 3,

    /// <summary>
    /// The project has been prematurely aborted or terminated by a participant.
    /// </summary>
    Canceled = 4
}
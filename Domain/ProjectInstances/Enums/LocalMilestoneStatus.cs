namespace AcademicGateway.Domain.ProjectInstances.Enums;

/// <summary>
/// Defines the runtime execution state machine options for an active project workspace milestone.
/// </summary>
public enum LocalMilestoneStatus
{
    /// <summary>
    /// The milestone has been initialized but is not yet active or scheduled.
    /// </summary>
    NotStarted = 0,

    /// <summary>
    /// The student is actively working on completing the milestone deliverables.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// The student has submitted the required deliverable and is awaiting review.
    /// </summary>
    Submitted = 2,

    /// <summary>
    /// The academic supervisor has evaluated the submission and finalized its grade.
    /// </summary>
    Graded = 3
}
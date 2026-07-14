namespace AcademicGateway.Domain.ProjectInstances.Enums;

/// <summary>
/// Defines the runtime execution lifecycle states for an individual local task instance.
/// </summary>
public enum LocalTaskStatus
{
    /// <summary>
    /// The task has been initialized but no student deliverable has been pushed yet.
    /// </summary>
    NotStarted = 0,

    /// <summary>
    /// A student deliverable payload has been successfully uploaded and is awaiting evaluation.
    /// </summary>
    Submitted = 1,

    /// <summary>
    /// An academic supervisor or authorized user has certified a grade and feedback for this task.
    /// </summary>
    Graded = 2
}
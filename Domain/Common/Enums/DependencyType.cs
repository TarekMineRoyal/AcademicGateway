namespace Domain.Common.Enums;

/// <summary>
/// Defines the type of dependency relationship between milestones.
/// </summary>
public enum DependencyType
{
    /// <summary>
    /// The predecessor must be finished before the successor can start.
    /// </summary>
    FinishToStart = 1,

    /// <summary>
    /// The predecessor must be started before the successor can start.
    /// </summary>
    StartToStart = 2
}
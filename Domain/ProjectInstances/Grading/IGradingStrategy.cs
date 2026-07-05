using System;
using System.Collections.Generic;

namespace AcademicGateway.Domain.ProjectInstances.Grading;

/// <summary>
/// Defines the core domain contract for hot-swappable grading strategies.
/// Governs execution-layer evaluation rules for individual milestones and handles macro-level final aggregate project scoring computations.
/// </summary>
public interface IGradingStrategy
{
    /// <summary>
    /// Gets the unique system tracking descriptor name identifying this concrete algorithmic strategy.
    /// </summary>
    string StrategyName { get; }

    /// <summary>
    /// Verifies whether a proposed individual milestone grade conforms strictly to the mathematical boundaries of this strategy.
    /// </summary>
    /// <param name="grade">The proposed numerical score assigned by an evaluator.</param>
    /// <returns>True if the grade score resides cleanly within allowable numerical boundaries; otherwise, false.</returns>
    bool IsValidMilestoneGrade(decimal grade);

    /// <summary>
    /// Evaluates whether a finalized individual milestone grade meets or surpasses passing thresholds.
    /// </summary>
    /// <param name="grade">The numerical score verified on the milestone entity.</param>
    /// <returns>True if the milestone calculation qualifies as passing; otherwise, false.</returns>
    bool IsPassingMilestone(decimal grade);

    /// <summary>
    /// Computes the final macro-level overall aggregate grade score for a project workspace instance based on its completed milestone network.
    /// </summary>
    /// <param name="milestones">The complete historical collection of snapshotted local milestones linked to the project aggregate root boundary.</param>
    /// <returns>The final calculated aggregate score for the entire project workspace runtime instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the provided milestones sequence container is uninitialized.</exception>
    /// <exception cref="InvalidOperationException">Thrown if unvalidated or ungraded milestone elements block macro evaluation passes.</exception>
    decimal CalculateFinalProjectGrade(IReadOnlyCollection<LocalMilestone> milestones);
}
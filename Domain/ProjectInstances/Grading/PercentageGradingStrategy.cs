using System;
using System.Collections.Generic;
using System.Linq;
using AcademicGateway.Domain.ProjectInstances.Enums;

namespace AcademicGateway.Domain.ProjectInstances.Grading;

/// <summary>
/// Implements a continuous percentage-based evaluation policy rule.
/// Task grades are numerical scores ranging from 0.00 to 100.00.
/// The macro-level project grade is determined from the bottom up using a hierarchical weighted rollup 
/// that factors in individual task weights and decoupled milestone grading weights.
/// </summary>
public class PercentageGradingStrategy : IGradingStrategy
{
    private const decimal MinimumScore = 0.00m;
    private const decimal MaximumScore = 100.00m;
    private const decimal PassingThreshold = 50.00m;

    /// <inheritdoc />
    public string StrategyName => "Percentage Continuous Scale (0-100)";

    /// <summary>
    /// Enforces that a percentage score falls within the continuous range of 0.00 to 100.00 inclusive.
    /// </summary>
    /// <param name="grade">The proposed numerical score assigned by an evaluator.</param>
    /// <returns>True if the score value resides cleanly within allowable boundaries; otherwise, false.</returns>
    public bool IsValidMilestoneGrade(decimal grade)
    {
        return grade >= MinimumScore && grade <= MaximumScore;
    }

    /// <summary>
    /// Evaluates if a given numerical score meets or exceeds the academic passing threshold (>= 50%).
    /// </summary>
    /// <param name="grade">The numerical score verified on the domain element.</param>
    /// <returns>True if the calculation qualifies as passing; otherwise, false.</returns>
    public bool IsPassingMilestone(decimal grade)
    {
        return grade >= PassingThreshold;
    }

    /// <summary>
    /// Computes the final macro-level overall aggregate grade score for a project workspace instance 
    /// using a bottom-up hierarchical weighted compound calculation across milestones and tasks.
    /// </summary>
    /// <param name="milestones">The complete historical collection of snapshotted local milestones linked to the project aggregate root.</param>
    /// <returns>The final calculated aggregate percentage score rounded precisely to two decimal places.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the milestones sequence container is uninitialized.</exception>
    /// <exception cref="InvalidOperationException">Thrown if any nested task across the project layout is ungraded or incomplete.</exception>
    public decimal CalculateFinalProjectGrade(IReadOnlyCollection<LocalMilestone> milestones)
    {
        if (milestones == null)
        {
            throw new ArgumentNullException(nameof(milestones), "Milestone tracking collection cannot be null.");
        }

        if (!milestones.Any())
        {
            throw new InvalidOperationException("Cannot compute macro percentage scores for an empty execution graph.");
        }

        // ---------------------------------------------------------------------
        // DOMAIN GUARD GATE: Ensure every nested task across all milestones is graded
        // ---------------------------------------------------------------------
        foreach (var milestone in milestones)
        {
            foreach (var task in milestone.LocalTasks)
            {
                if (task.Status != LocalTaskStatus.Graded || !task.Grade.HasValue)
                {
                    throw new InvalidOperationException(
                        "Macro Evaluation Denied: Final project scoring calculations are blocked " +
                        $"until every individual task within milestone '{milestone.TitleSnapshot}' has been evaluated and graded.");
                }
            }
        }

        // ---------------------------------------------------------------------
        // HIERARCHICAL WEIGHTED ROLLUP CALCULATION
        // ---------------------------------------------------------------------
        decimal finalProjectGrade = 0.00m;

        foreach (var milestone in milestones)
        {
            // Step 1: Calculate this milestone's inner composite grade score based on its tasks
            decimal milestoneCompositeScore = 0.00m;

            foreach (var task in milestone.LocalTasks)
            {
                milestoneCompositeScore += task.Grade!.Value * (task.Weight / 100m);
            }

            // Step 2: Compound this milestone's score into the final project macro grade using its decoupled GradingWeight
            finalProjectGrade += milestoneCompositeScore * (milestone.GradingWeight / 100m);
        }

        // Round to two decimal places for database scale precision compliance
        return Math.Round(finalProjectGrade, 2, MidpointRounding.AwayFromZero);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using AcademicGateway.Domain.ProjectInstances.Enums;

namespace AcademicGateway.Domain.ProjectInstances.Grading;

/// <summary>
/// Implements a binary Pass/Fail evaluation policy rule.
/// Under this domain strategy, evaluation remains strictly binary. The entire project only passes 
/// if every single task under every single milestone has successfully achieved a passing grade.
/// </summary>
public class PassFailGradingStrategy : IGradingStrategy
{
    private const decimal FailScore = 0.00m;
    private const decimal PassScore = 100.00m;

    /// <inheritdoc />
    public string StrategyName => "Pass/Fail Binary Scale";

    /// <summary>
    /// Enforces that a Pass/Fail metric must be exactly 0 (Fail) or 100 (Pass).
    /// </summary>
    /// <param name="grade">The numerical score value being checked.</param>
    /// <returns>True if the value represents an allowable score boundary token; otherwise, false.</returns>
    public bool IsValidMilestoneGrade(decimal grade)
    {
        return grade == FailScore || grade == PassScore;
    }

    /// <summary>
    /// Evaluates if the numerical task or milestone score equates to a passing state.
    /// </summary>
    /// <param name="grade">The numerical score to assess.</param>
    /// <returns>True if the score qualifies as passing; otherwise, false.</returns>
    public bool IsPassingMilestone(decimal grade)
    {
        return grade == PassScore;
    }

    /// <summary>
    /// Computes the final aggregate macro project grade across the execution hierarchy graph.
    /// If any single nested task has a grade equal to FailScore (0.00), the entire project fails.
    /// </summary>
    /// <param name="milestones">The complete historical collection of snapshotted local milestones linked to the project aggregate root.</param>
    /// <returns>The binary aggregate score (0.00 or 100.00) assigned to the project workspace instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the milestones collection sequence container is uninitialized.</exception>
    /// <exception cref="InvalidOperationException">Thrown if any nested task across the project layout is ungraded or incomplete.</exception>
    public decimal CalculateFinalProjectGrade(IReadOnlyCollection<LocalMilestone> milestones)
    {
        if (milestones == null)
        {
            throw new ArgumentNullException(nameof(milestones), "Milestone tracking collection cannot be null.");
        }

        if (!milestones.Any())
        {
            throw new InvalidOperationException("Cannot compute macro scores for an empty execution graph.");
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
        // ROLLUP EVALUATION FORMULA: Short-circuit on any task failure
        // ---------------------------------------------------------------------
        foreach (var milestone in milestones)
        {
            foreach (var task in milestone.LocalTasks)
            {
                if (task.Grade == FailScore)
                {
                    return FailScore;
                }
            }
        }

        return PassScore;
    }
}
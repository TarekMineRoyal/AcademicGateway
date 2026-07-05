using System;
using System.Collections.Generic;
using System.Linq;
using AcademicGateway.Domain.ProjectInstances.Enums;

namespace AcademicGateway.Domain.ProjectInstances.Grading;

/// <summary>
/// Implements a continuous percentage-based evaluation policy rule.
/// Milestone grades are numerical scores ranging from 0.00 to 100.00.
/// The macro-level project grade is determined by computing the straight arithmetic average of all milestone scores.
/// </summary>
public class PercentageGradingStrategy : IGradingStrategy
{
    private const decimal MinimumScore = 0.00m;
    private const decimal MaximumScore = 100.00m;
    private const decimal PassingThreshold = 50.00m;

    /// <inheritdoc />
    public string StrategyName => "Percentage Continuous Scale (0-100)";

    /// <summary>
    /// Enforces that a percentage grade must fall within the continuous range of 0.00 to 100.00 inclusive.
    /// </summary>
    public bool IsValidMilestoneGrade(decimal grade)
    {
        return grade >= MinimumScore && grade <= MaximumScore;
    }

    /// <summary>
    /// Evaluates if the numerical milestone score meets or exceeds the academic passing threshold (>= 50%).
    /// </summary>
    public bool IsPassingMilestone(decimal grade)
    {
        return grade >= PassingThreshold;
    }

    /// <summary>
    /// Computes the final aggregate project grade by calculating the arithmetic mean across all milestone nodes.
    /// </summary>
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

        // Domain Guard: Every milestone must be evaluated before the macro project state can be finalized
        var ungradedMilestoneExists = milestones.Any(m => m.Status != LocalMilestoneStatus.Graded || !m.Grade.HasValue);
        if (ungradedMilestoneExists)
        {
            throw new InvalidOperationException(
                "Macro Evaluation Denied: Final project scoring calculations are blocked " +
                "until every local milestone has been evaluated and graded by a supervisor.");
        }

        // Aggregate Rule: Calculate the arithmetic mean of all individual milestone grades
        decimal totalScore = 0.00m;
        foreach (var milestone in milestones)
        {
            totalScore += milestone.Grade!.Value;
        }

        decimal averageScore = totalScore / milestones.Count;

        // Round to two decimal places for database scale precision compliance
        return Math.Round(averageScore, 2, MidpointRounding.AwayFromZero);
    }
}
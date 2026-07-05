using System;
using System.Collections.Generic;
using System.Linq;
using AcademicGateway.Domain.ProjectInstances.Enums;

namespace AcademicGateway.Domain.ProjectInstances.Grading;

/// <summary>
/// Implements a binary Pass/Fail evaluation policy rule.
/// Under this domain strategy, milestone grades are strictly binary (0 or 100), 
/// and the macro-level project only passes if every single milestone achieves a passing score.
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
    public bool IsValidMilestoneGrade(decimal grade)
    {
        return grade == FailScore || grade == PassScore;
    }

    /// <summary>
    /// Evaluates if the numerical milestone score equates to a passing state.
    /// </summary>
    public bool IsPassingMilestone(decimal grade)
    {
        return grade == PassScore;
    }

    /// <summary>
    /// Computes the final aggregate project grade. 
    /// If any single milestone is marked as a Fail (0), the entire project workspace receives a Fail (0).
    /// </summary>
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

        // Domain Guard: Every milestone must be evaluated before the macro project state can be finalized
        var ungradedMilestoneExists = milestones.Any(m => m.Status != LocalMilestoneStatus.Graded || !m.Grade.HasValue);
        if (ungradedMilestoneExists)
        {
            throw new InvalidOperationException(
                "Macro Evaluation Denied: Final project scoring calculations are blocked " +
                "until every local milestone has been evaluated and graded by a supervisor.");
        }

        // Compiler Safety Adjustment: Comparing nullable decimal directly to decimal constant 
        // to cleanly eliminate CS8629 warnings while retaining identical evaluation logic.
        var hasAnyFailures = milestones.Any(m => m.Grade == FailScore);

        return hasAnyFailures ? FailScore : PassScore;
    }
}
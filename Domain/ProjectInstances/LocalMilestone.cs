using System;
using System.Collections.Generic;
using System.Linq;
using AcademicGateway.Domain.Common;
using AcademicGateway.Domain.Common.Enums;
using AcademicGateway.Domain.ProjectInstances.Enums;

namespace AcademicGateway.Domain.ProjectInstances;

/// <summary>
/// Represents an isolated, active runtime milestone managed inside a student's project workspace instance.
/// Holds scheduling timelines, state changes, polymorphic submission strings, and evaluation metrics.
/// </summary>
public class LocalMilestone : BaseEntity
{
    /// <summary>
    /// Gets the unique tracking identifier for this runtime milestone instance.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the unique identifier of the parent ProjectInstance aggregate root.
    /// </summary>
    public Guid ProjectInstanceId { get; private set; }

    /// <summary>
    /// Gets the static descriptive snapshot title copied from the source blueprint definition.
    /// </summary>
    public string TitleSnapshot { get; private set; }

    /// <summary>
    /// Gets the static descriptive instructions copy fetched from the template.
    /// </summary>
    public string DescriptionSnapshot { get; private set; }

    /// <summary>
    /// Gets the effort estimation requirement value measured in hours.
    /// </summary>
    public decimal ExpectedEffortInHours { get; private set; }

    /// <summary>
    /// Gets the format category constraint mapped for student submission handling.
    /// </summary>
    public DeliverableType RequiredDeliverableType { get; private set; }

    /// <summary>
    /// Gets the current state within the individual task execution state machine.
    /// </summary>
    public LocalMilestoneStatus Status { get; internal set; } // Settable within the aggregate root boundaries

    /// <summary>
    /// Gets the student-assigned start date for this execution leg. Nullable until scheduled per Rule 3.
    /// </summary>
    public DateTime? ScheduledStartDate { get; internal set; }

    /// <summary>
    /// Gets the student-assigned deadline target for this execution leg. Nullable until scheduled per Rule 3.
    /// </summary>
    public DateTime? ScheduledEndDate { get; internal set; }

    /// <summary>
    /// Gets the polymorphic raw submission content payload text (e.g. URL string, text summary, file token locator).
    /// </summary>
    public string? SubmissionPayload { get; internal set; }

    /// <summary>
    /// Gets the exact tracking timestamp when the student completed the deliverable push.
    /// </summary>
    public DateTime? SubmittedAt { get; internal set; }

    /// <summary>
    /// Gets the final numerical score value awarded during professor evaluation.
    /// </summary>
    public decimal? Grade { get; internal set; }

    /// <summary>
    /// Gets the formal evaluation audit feedback commentary logged by the grading mentor.
    /// </summary>
    public string? EvaluationFeedback { get; internal set; }

    /// <summary>
    /// Gets the timestamp tracking when evaluation calculations were certified.
    /// </summary>
    public DateTime? GradedAt { get; internal set; }

    /// <summary>
    /// Backing tracking structure managing the prerequisite links attached to this node.
    /// </summary>
    private readonly List<LocalMilestoneDependency> _inboundDependencies = new();

    /// <summary>
    /// Exposes incoming dependency boundaries as an encapsulated read-only sequence structure.
    /// </summary>
    public IReadOnlyCollection<LocalMilestoneDependency> InboundDependencies => _inboundDependencies.AsReadOnly();

    /// <summary>
    /// Parameterless constructor required by EF Core for persistence materialization loops.
    /// </summary>
    private LocalMilestone()
    {
        TitleSnapshot = null!;
        DescriptionSnapshot = null!;
    }

    /// <summary>
    /// Factory-scoped instantiation constructor designed to generate an isolated execution milestone.
    /// </summary>
    public LocalMilestone(
        Guid projectInstanceId,
        string titleSnapshot,
        string descriptionSnapshot,
        decimal expectedEffortInHours,
        DeliverableType requiredDeliverableType)
    {
        Id = Guid.NewGuid();
        ProjectInstanceId = projectInstanceId;
        TitleSnapshot = titleSnapshot.Trim();
        DescriptionSnapshot = descriptionSnapshot.Trim();
        ExpectedEffortInHours = expectedEffortInHours;
        RequiredDeliverableType = requiredDeliverableType;
        Status = LocalMilestoneStatus.NotStarted;
    }

    /// <summary>
    /// Injects a runtime predecessor constraint edge into this node's dependency list collection.
    /// </summary>
    internal void AddInboundDependency(Guid predecessorId, DependencyType type)
    {
        var dependency = new LocalMilestoneDependency(predecessorId, this.Id, type);

        if (!_inboundDependencies.Any(d => d.PredecessorId == predecessorId))
        {
            _inboundDependencies.Add(dependency);
        }
    }
}
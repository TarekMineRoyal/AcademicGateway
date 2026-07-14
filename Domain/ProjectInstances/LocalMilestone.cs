using System;
using System.Collections.Generic;
using System.Linq;
using AcademicGateway.Domain.Common;
using AcademicGateway.Domain.Common.Enums;
using AcademicGateway.Domain.ProjectInstances.Enums;
using AcademicGateway.Domain.ProjectInstances.Grading;

namespace AcademicGateway.Domain.ProjectInstances;

/// <summary>
/// Represents an isolated, active runtime milestone managed inside a student's project workspace instance.
/// Act as a container for nested runtime tasks and tracks individual scheduling timelines and conversation logs.
/// </summary>
public class LocalMilestone : BaseEntity
{
    private readonly List<LocalMilestoneDependency> _inboundDependencies = new();
    private readonly List<MilestoneComment> _comments = new();
    private readonly List<LocalTask> _localTasks = new();

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
    /// Gets the operational work/progress weight of this milestone out of 100% total project effort.
    /// </summary>
    public decimal WbsWeight { get; internal set; }

    /// <summary>
    /// Gets the academic grading weight of this milestone out of 100% total project score.
    /// </summary>
    public decimal GradingWeight { get; internal set; }

    /// <summary>
    /// Gets the current state within the individual milestone execution state machine.
    /// </summary>
    public LocalMilestoneStatus Status { get; internal set; }

    /// <summary>
    /// Gets the student-assigned start date for this execution leg. Nullable until scheduled.
    /// </summary>
    public DateTime? ScheduledStartDate { get; internal set; }

    /// <summary>
    /// Gets the student-assigned deadline target for this execution leg. Nullable until scheduled.
    /// </summary>
    public DateTime? ScheduledEndDate { get; internal set; }

    /// <summary>
    /// Exposes incoming dependency boundaries as an encapsulated read-only sequence structure.
    /// </summary>
    public IReadOnlyCollection<LocalMilestoneDependency> InboundDependencies => _inboundDependencies.AsReadOnly();

    /// <summary>
    /// Exposes timestamped cross-role conversation commentary logged within this milestone workspace channel.
    /// </summary>
    public IReadOnlyCollection<MilestoneComment> Comments => _comments.AsReadOnly();

    /// <summary>
    /// Exposes the child runtime tasks as a read-only collection to preserve domain encapsulation.
    /// </summary>
    public IReadOnlyCollection<LocalTask> LocalTasks => _localTasks.AsReadOnly();

    /// <summary>
    /// Evaluates whether the nested task percentage weights form a complete 100% distribution.
    /// </summary>
    public bool IsWbsBalanced => _localTasks.Sum(t => t.Weight) == 100m;

    /// <summary>
    /// Parameterless constructor required by EF Core for persistence materialization loops.
    /// </summary>
    private LocalMilestone()
    {
        TitleSnapshot = null!;
        DescriptionSnapshot = null!;
    }

    /// <summary>
    /// Factory-scoped instantiation constructor designed to generate an isolated execution milestone container.
    /// </summary>
    /// <param name="projectInstanceId">The unique identifier of the parent project workspace root.</param>
    /// <param name="titleSnapshot">The static descriptive blueprint headline copy.</param>
    /// <param name="descriptionSnapshot">The static descriptive blueprint instruction context copy.</param>
    /// <param name="expectedEffortInHours">The estimated work tracking duration in hours.</param>
    /// <param name="wbsWeight">The operational progress weight percentage assigned to this milestone track.</param>
    /// <param name="gradingWeight">The academic grade evaluation weight percentage assigned to this milestone track.</param>
    public LocalMilestone(
        Guid projectInstanceId,
        string titleSnapshot,
        string descriptionSnapshot,
        decimal expectedEffortInHours,
        decimal wbsWeight,
        decimal gradingWeight)
    {
        Id = Guid.NewGuid();
        ProjectInstanceId = projectInstanceId;
        TitleSnapshot = titleSnapshot.Trim();
        DescriptionSnapshot = descriptionSnapshot.Trim();
        ExpectedEffortInHours = expectedEffortInHours;
        WbsWeight = wbsWeight;
        GradingWeight = gradingWeight;
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

    /// <summary>
    /// Seeds the deep-cloned runtime task instances into this milestone container during snapshot manufacturing.
    /// </summary>
    internal void SeedClonedTasks(IEnumerable<LocalTask> tasks)
    {
        _localTasks.AddRange(tasks);
        UpdateStatusFromTasks();
    }

    // =========================================================================
    // DDD ENCAPSULATION ROUTERS (DELEGATING TO CHILD TASKS)
    // =========================================================================

    /// <summary>
    /// Locates a targeted nested task item and routes the student deliverable submission payload down to it.
    /// </summary>
    internal void SubmitTaskDeliverable(Guid taskId, string payload, DateTime utcNow)
    {
        var task = _localTasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null)
        {
            throw new KeyNotFoundException($"Local Task with ID '{taskId}' was not found within this milestone context.");
        }

        task.SubmitDeliverable(payload, utcNow);
        UpdateStatusFromTasks();
    }

    /// <summary>
    /// Locates a targeted nested task item and routes the evaluation score and critique data down to it.
    /// </summary>
    internal void EvaluateTaskSubmission(Guid taskId, decimal grade, string? feedback, IGradingStrategy gradingStrategy, DateTime utcNow)
    {
        var task = _localTasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null)
        {
            throw new KeyNotFoundException($"Local Task with ID '{taskId}' was not found within this milestone context.");
        }

        task.EvaluateSubmission(grade, feedback, gradingStrategy, utcNow);
        UpdateStatusFromTasks();
    }

    /// <summary>
    /// Automatically calculates and updates the state machine of this milestone based on the collective states of its nested tasks.
    /// </summary>
    internal void UpdateStatusFromTasks()
    {
        if (!_localTasks.Any())
        {
            return;
        }

        if (_localTasks.All(t => t.Status == LocalTaskStatus.Graded))
        {
            Status = LocalMilestoneStatus.Graded;
        }
        else if (_localTasks.All(t => t.Status == LocalTaskStatus.Submitted || t.Status == LocalTaskStatus.Graded))
        {
            Status = LocalMilestoneStatus.Submitted;
        }
        else if (_localTasks.Any(t => t.Status == LocalTaskStatus.Submitted || t.Status == LocalTaskStatus.Graded) || ScheduledStartDate.HasValue)
        {
            Status = LocalMilestoneStatus.InProgress;
        }
        else
        {
            Status = LocalMilestoneStatus.NotStarted;
        }
    }

    // =========================================================================
    // CROSS-ROLE MILESTONE DISCUSSION ENGINE MUTATOR
    // =========================================================================

    /// <summary>
    /// Appends a new immutable collaboration commentary entry log directly onto this milestone tracking lane.
    /// </summary>
    internal void AddComment(Guid authorId, string authorIdentitySnapshot, string content, DateTime utcNow)
    {
        var comment = new MilestoneComment(this.Id, authorId, authorIdentitySnapshot, content, utcNow);
        _comments.Add(comment);
    }
}
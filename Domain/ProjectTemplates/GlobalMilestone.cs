using System;
using System.Collections.Generic;
using System.Linq;
using AcademicGateway.Domain.Common;
using AcademicGateway.Domain.Common.Enums;
using AcademicGateway.Domain.ProjectTemplates.Exceptions;

namespace AcademicGateway.Domain.ProjectTemplates;

/// <summary>
/// Represents a blueprint milestone defined by a Provider as part of a project template.
/// This acts as a static configuration definition and does not contain execution state.
/// </summary>
public class GlobalMilestone : BaseEntity
{
    private readonly List<MilestoneDependency> _inboundDependencies = new();
    private readonly List<GlobalTask> _globalTasks = new();

    /// <summary>
    /// Gets the unique identifier for this specific global milestone blueprint.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the unique identifier of the parent ProjectTemplate aggregate root.
    /// </summary>
    public Guid ProjectTemplateId { get; private set; }

    /// <summary>
    /// Gets the title of the milestone.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Gets the detailed description or instructions for the milestone.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the estimated effort required to complete the milestone, measured in hours.
    /// Used for effort-based scheduling calculations.
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
    /// Exposes the inbound dependencies (predecessors) as a read-only collection 
    /// to preserve domain encapsulation.
    /// </summary>
    public IReadOnlyCollection<MilestoneDependency> InboundDependencies => _inboundDependencies.AsReadOnly();

    /// <summary>
    /// Exposes the child tasks as a read-only collection to preserve domain encapsulation.
    /// </summary>
    public IReadOnlyCollection<GlobalTask> GlobalTasks => _globalTasks.AsReadOnly();

    /// <summary>
    /// Parameterless constructor required by EF Core for materialization.
    /// </summary>
    private GlobalMilestone()
    {
        Title = null!;
        Description = null!;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalMilestone"/> class.
    /// </summary>
    /// <param name="projectTemplateId">The identifier of the owning template.</param>
    /// <param name="title">The milestone title.</param>
    /// <param name="description">The milestone description.</param>
    /// <param name="expectedEffortInHours">The estimated effort in hours.</param>
    /// <param name="wbsWeight">The operational breakdown structure effort weight percentage.</param>
    /// <param name="gradingWeight">The academic assessment weight percentage.</param>
    public GlobalMilestone(
        Guid projectTemplateId,
        string title,
        string description,
        decimal expectedEffortInHours,
        decimal wbsWeight,
        decimal gradingWeight)
    {
        Id = Guid.NewGuid();
        ProjectTemplateId = projectTemplateId;
        Title = title;
        Description = description;
        ExpectedEffortInHours = expectedEffortInHours;
        WbsWeight = wbsWeight;
        GradingWeight = gradingWeight;
    }

    /// <summary>
    /// Declares a predecessor constraint that must be satisfied before this milestone can proceed.
    /// </summary>
    /// <param name="predecessorId">The unique identifier of the milestone that must come first.</param>
    /// <param name="type">The behavioral constraint type (e.g., FinishToStart).</param>
    public void AddPredecessor(Guid predecessorId, DependencyType type)
    {
        var dependency = new MilestoneDependency(predecessorId, this.Id, type);

        if (!_inboundDependencies.Any(d => d.PredecessorId == predecessorId))
        {
            _inboundDependencies.Add(dependency);
        }
    }

    /// <summary>
    /// Updates the core metadata criteria and effort configurations for this global milestone blueprint node.
    /// Marked as internal to guarantee mutations are strictly driven by the parent ProjectTemplate aggregate root.
    /// </summary>
    /// <param name="title">The newly updated structural headline title of the milestone.</param>
    /// <param name="description">The revised conceptual context mapping work item goals.</param>
    /// <param name="expectedEffortInHours">The nominal estimation workload metrics in execution hours.</param>
    /// <param name="wbsWeight">The operational weight percentage assigned to this milestone node config.</param>
    /// <param name="gradingWeight">The academic evaluation weight percentage assigned to this milestone node config.</param>
    /// <exception cref="InvalidTemplateDetailsException">Thrown when title strings or effort durations fail verification.</exception>
    internal void UpdateDetails(
        string title,
        string description,
        decimal expectedEffortInHours,
        decimal wbsWeight,
        decimal gradingWeight)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidTemplateDetailsException("Project template milestone title cannot be empty or whitespace.");
        }

        if (expectedEffortInHours <= 0)
        {
            throw new InvalidTemplateDetailsException("Expected effort must be greater than zero hours.");
        }

        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        ExpectedEffortInHours = expectedEffortInHours;
        WbsWeight = wbsWeight;
        GradingWeight = gradingWeight;
    }

    /// <summary>
    /// Severs an inbound sequencing dependency restriction link originating from a specified predecessor node.
    /// Marked as internal to guarantee graph edge deletions are strictly managed by the parent aggregate context layout.
    /// </summary>
    /// <param name="predecessorId">The primary identifier of the prerequisite milestone constraint edge to clean up.</param>
    /// <returns>True if an established dependency link matching the tracking parameters was successfully detached; otherwise false.</returns>
    internal bool RemovePredecessor(Guid predecessorId)
    {
        var dependency = _inboundDependencies.FirstOrDefault(d => d.PredecessorId == predecessorId);
        if (dependency != null)
        {
            _inboundDependencies.Remove(dependency);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Appends a nested blueprint task configuration to this milestone container node.
    /// </summary>
    internal void AddTask(string title, string description, decimal weight, DeliverableType requiredDeliverableType)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidTemplateDetailsException("Project template task title cannot be empty or whitespace.");
        }

        var task = new GlobalTask(this.Id, title.Trim(), description?.Trim() ?? string.Empty, weight, requiredDeliverableType);
        _globalTasks.Add(task);
    }

    /// <summary>
    /// Updates an existing child blueprint task node with modified metadata or allocation weights.
    /// </summary>
    internal void UpdateTask(Guid taskId, string title, string description, decimal weight, DeliverableType requiredDeliverableType)
    {
        var task = _globalTasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null)
        {
            throw new InvalidTemplateDetailsException("The requested task blueprint node does not exist within this milestone configuration context.");
        }

        task.UpdateDetails(title, description, weight, requiredDeliverableType);
    }

    /// <summary>
    /// Safely purges a child task node definition from this milestone configuration container context.
    /// </summary>
    internal void RemoveTask(Guid taskId)
    {
        var task = _globalTasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null)
        {
            throw new InvalidTemplateDetailsException("The requested task blueprint node does not exist within this milestone configuration context.");
        }

        _globalTasks.Remove(task);
    }
}
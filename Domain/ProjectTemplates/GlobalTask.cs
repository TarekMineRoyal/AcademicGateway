using System;
using AcademicGateway.Domain.Common;
using AcademicGateway.Domain.Common.Enums;
using AcademicGateway.Domain.ProjectTemplates.Exceptions;

namespace AcademicGateway.Domain.ProjectTemplates;

/// <summary>
/// Represents a nested blueprint task configuration within a milestone.
/// Tracks its own specific deliverable requirements and relative weight distribution.
/// </summary>
public class GlobalTask : BaseEntity
{
    /// <summary>
    /// Gets the unique identifier for this specific global task blueprint.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the unique identifier of the parent GlobalMilestone container.
    /// </summary>
    public Guid GlobalMilestoneId { get; private set; }

    /// <summary>
    /// Gets the headline title of the task blueprint.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Gets the detailed conceptual instructions or scope requirements for the task.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the percentage share weight of this task within its parent milestone container.
    /// </summary>
    public decimal Weight { get; private set; }

    /// <summary>
    /// Gets the structural format constraint type required for this task's deliverable submission.
    /// </summary>
    public DeliverableType RequiredDeliverableType { get; private set; }

    /// <summary>
    /// Parameterless constructor required by EF Core for persistence materialization.
    /// Bypasses domain invariant requirements during data hydration loops.
    /// </summary>
    private GlobalTask()
    {
        Title = null!;
        Description = null!;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalTask"/> child domain entity.
    /// Constructor is marked internal to force instantiation solely through the domain aggregate boundary.
    /// </summary>
    /// <param name="globalMilestoneId">The tracking identifier of the parent milestone.</param>
    /// <param name="title">The structural headline title of the task item.</param>
    /// <param name="description">The background details or completion standards mapping requirements.</param>
    /// <param name="weight">The operational progress percentage weight relative to the parent milestone.</param>
    /// <param name="requiredDeliverableType">The formatting rule restriction token expected for tracking submissions.</param>
    internal GlobalTask(
        Guid globalMilestoneId,
        string title,
        string description,
        decimal weight,
        DeliverableType requiredDeliverableType)
    {
        Id = Guid.NewGuid();
        GlobalMilestoneId = globalMilestoneId;
        Title = title;
        Description = description;
        Weight = weight;
        RequiredDeliverableType = requiredDeliverableType;
    }

    /// <summary>
    /// Updates the core metadata criteria and weight allocations for this global task node.
    /// Marked as internal to guarantee mutations are strictly driven by the parent aggregate root context.
    /// </summary>
    /// <param name="title">The newly updated structural headline title of the task.</param>
    /// <param name="description">The revised conceptual context mapping task execution expectations.</param>
    /// <param name="weight">The operational progress weight assigned to this task node configuration.</param>
    /// <param name="requiredDeliverableType">The explicit deliverable tracking submission constraint rule token.</param>
    /// <exception cref="InvalidTemplateDetailsException">Thrown when title strings or text fields fail validation verification.</exception>
    internal void UpdateDetails(
        string title,
        string description,
        decimal weight,
        DeliverableType requiredDeliverableType)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidTemplateDetailsException("Project template task title cannot be empty or whitespace.");
        }

        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        Weight = weight;
        RequiredDeliverableType = requiredDeliverableType;
    }
}
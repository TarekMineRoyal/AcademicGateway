using AcademicGateway.Domain.Common;
using Domain.Common.Enums;

namespace AcademicGateway.Domain.ProjectTemplates;

/// <summary>
/// Represents a blueprint milestone defined by a Provider as part of a project template.
/// This acts as a static configuration definition and does not contain execution state.
/// </summary>
public class GlobalMilestone : BaseEntity
{
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
    /// Gets the expected format type for the deliverable submission.
    /// </summary>
    public DeliverableType RequiredDeliverableType { get; private set; }

    /// <summary>
    /// Internal collection tracking the incoming constraints that must be satisfied 
    /// before this milestone can proceed.
    /// </summary>
    private readonly List<MilestoneDependency> _inboundDependencies = new();

    /// <summary>
    /// Exposes the inbound dependencies (predecessors) as a read-only collection 
    /// to preserve domain encapsulation.
    /// </summary>
    public IReadOnlyCollection<MilestoneDependency> InboundDependencies => _inboundDependencies.AsReadOnly();

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
    /// <param name="requiredDeliverableType">The formatting rule for submission.</param>
    public GlobalMilestone(
        Guid projectTemplateId,
        string title,
        string description,
        decimal expectedEffortInHours,
        DeliverableType requiredDeliverableType)
    {
        Id = Guid.NewGuid();
        ProjectTemplateId = projectTemplateId;
        Title = title;
        Description = description;
        ExpectedEffortInHours = expectedEffortInHours;
        RequiredDeliverableType = requiredDeliverableType;
    }

    /// <summary>
    /// Declares a predecessor constraint that must be satisfied before this milestone can proceed.
    /// </summary>
    /// <param name="predecessorId">The unique identifier of the milestone that must come first.</param>
    /// <param name="type">The behavioral constraint type (e.g., FinishToStart).</param>
    public void AddPredecessor(Guid predecessorId, DependencyType type)
    {
        // The domain guard for self-dependency is already enforced inside the MilestoneDependency constructor.
        var dependency = new MilestoneDependency(predecessorId, this.Id, type);

        // Prevent duplicate dependency constraints
        if (!_inboundDependencies.Any(d => d.PredecessorId == predecessorId))
        {
            _inboundDependencies.Add(dependency);
        }
    }
}
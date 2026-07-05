using AcademicGateway.Domain.Common.Enums;

namespace AcademicGateway.Domain.ProjectTemplates;

/// <summary>
/// Represents a dependency constraint between two global milestones.
/// </summary>
public class MilestoneDependency
{
    /// <summary>
    /// The unique identifier of the predecessor milestone.
    /// </summary>
    public Guid PredecessorId { get; private set; }

    /// <summary>
    /// The unique identifier of the successor milestone.
    /// </summary>
    public Guid SuccessorId { get; private set; }

    /// <summary>
    /// The type of relationship constraint.
    /// </summary>
    public DependencyType Type { get; private set; }

    // EF Core Constructor
    private MilestoneDependency() { }

    public MilestoneDependency(Guid predecessorId, Guid successorId, DependencyType type)
    {
        if (predecessorId == Guid.Empty || successorId == Guid.Empty)
        {
            throw new ArgumentException("Milestone IDs cannot be empty.");
        }

        if (predecessorId == successorId)
        {
            throw new InvalidOperationException("A milestone cannot depend on itself (circular dependency guard).");
        }

        PredecessorId = predecessorId;
        SuccessorId = successorId;
        Type = type;
    }
}
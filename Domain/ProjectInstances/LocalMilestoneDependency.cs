using System;
using AcademicGateway.Domain.Common.Enums;

namespace AcademicGateway.Domain.ProjectInstances;

/// <summary>
/// Represents a concrete runtime dependency constraint edge between two local execution milestones.
/// Completely decoupled from template tracking records to protect runtime immutability.
/// </summary>
public class LocalMilestoneDependency
{
    /// <summary>
    /// Gets the unique runtime identifier of the preceding milestone.
    /// </summary>
    public Guid PredecessorId { get; private set; }

    /// <summary>
    /// Gets the unique runtime identifier of the dependent downstream milestone.
    /// </summary>
    public Guid SuccessorId { get; private set; }

    /// <summary>
    /// Gets the specific constraint rule applied to this edge (e.g. FinishToStart, StartToStart).
    /// </summary>
    public DependencyType Type { get; private set; }

    /// <summary>
    /// Parameterless constructor required by EF Core for persistence hydration.
    /// </summary>
    private LocalMilestoneDependency() { }

    /// <summary>
    /// Initializes a new runtime dependency constraint between two local milestones.
    /// </summary>
    /// <param name="predecessorId">The identifier of the prerequisite milestone.</param>
    /// <param name="successorId">The identifier of the dependent milestone.</param>
    /// <param name="type">The constraint relationship type layout.</param>
    /// <exception cref="ArgumentException">Thrown if keys are default empty states.</exception>
    /// <exception cref="InvalidOperationException">Thrown if a self-referential graph loop is attempted.</exception>
    public LocalMilestoneDependency(Guid predecessorId, Guid successorId, DependencyType type)
    {
        if (predecessorId == Guid.Empty || successorId == Guid.Empty)
        {
            throw new ArgumentException("Local milestone dependency identifiers cannot be empty.");
        }

        if (predecessorId == successorId)
        {
            throw new InvalidOperationException("Execution Graph Guard: A local milestone cannot depend on itself.");
        }

        PredecessorId = predecessorId;
        SuccessorId = successorId;
        Type = type;
    }
}
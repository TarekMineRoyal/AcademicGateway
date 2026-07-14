using System;
using System.Collections.Generic;
using System.Linq;
using AcademicGateway.Domain.ProjectTemplates;

namespace AcademicGateway.Domain.ProjectInstances.Services;

/// <summary>
/// Domain Service implementing the Factory Pattern to handle blueprint-to-execution snapshot isolation logic.
/// Responsible for deep-cloning global milestones into local milestones along with their nested tasks,
/// and re-mapping the Directed Acyclic Graph (DAG) dependency edges across entirely new primary key identifier spaces.
/// </summary>
public class LocalMilestoneFactory
{
    /// <summary>
    /// Deep-clones a collection of blueprint global milestones and their child tasks into isolated runtime execution elements,
    /// cleanly preserving multi-parent dependency graph topologies across separate ID vocabularies.
    /// </summary>
    /// <param name="projectInstanceId">The unique target identifier of the parent ProjectInstance aggregate root.</param>
    /// <param name="globalMilestones">The read-only collection of source blueprint milestones fetched from the template aggregate root.</param>
    /// <returns>A fully hydrated, structurally verified sequence of local execution milestones ready for aggregate seeding.</returns>
    /// <exception cref="ArgumentException">Thrown if parent tracking identifiers are uninitialized.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the source graph references missing predecessor elements.</exception>
    public IEnumerable<LocalMilestone> CreateLocalMilestonesSnapshot(
        Guid projectInstanceId,
        IReadOnlyCollection<GlobalMilestone> globalMilestones)
    {
        if (projectInstanceId == Guid.Empty)
        {
            throw new ArgumentException("Target project instance identifier cannot be empty.", nameof(projectInstanceId));
        }

        if (globalMilestones == null || !globalMilestones.Any())
        {
            return Enumerable.Empty<LocalMilestone>();
        }

        // ---------------------------------------------------------------------
        // PASS 1: Manufacture Isolated Nodes, Clone Tasks & Hydrate Map Dictionary
        // ---------------------------------------------------------------------
        // Key: Source GlobalMilestone ID, Value: Cloned LocalMilestone Entity Instance
        var translationMatrix = new Dictionary<Guid, LocalMilestone>();

        foreach (var globalMilestone in globalMilestones)
        {
            // Instantiate the execution milestone with split operational and grading weights
            var localMilestone = new LocalMilestone(
                projectInstanceId,
                globalMilestone.Title,
                globalMilestone.Description,
                globalMilestone.ExpectedEffortInHours,
                globalMilestone.WbsWeight,
                globalMilestone.GradingWeight);

            // Deep-clone nested blueprint tasks down into the runtime task sub-layer
            var localTasksList = new List<LocalTask>();
            foreach (var globalTask in globalMilestone.GlobalTasks)
            {
                var localTask = new LocalTask(
                    localMilestone.Id,
                    globalTask.Title,
                    globalTask.Description,
                    globalTask.Weight,
                    globalTask.RequiredDeliverableType);

                localTasksList.Add(localTask);
            }

            // Seed the manufactured tasks into the local milestone entity container
            localMilestone.SeedClonedTasks(localTasksList);

            translationMatrix.Add(globalMilestone.Id, localMilestone);
        }

        // ---------------------------------------------------------------------
        // PASS 2: Re-Map Directed Graph Edge Constraints onto Cloned Vocabularies
        // ---------------------------------------------------------------------
        foreach (var globalMilestone in globalMilestones)
        {
            // Resolve the matching target execution milestone node from the dictionary matrix
            var currentLocalSuccessor = translationMatrix[globalMilestone.Id];

            foreach (var blueprintDependency in globalMilestone.InboundDependencies)
            {
                // Verify that the predecessor referenced by the blueprint is present in our translation map
                if (!translationMatrix.TryGetValue(blueprintDependency.PredecessorId, out var localPredecessor))
                {
                    throw new InvalidOperationException(
                        $"Snapshot Generation Failure: Predecessor node '{blueprintDependency.PredecessorId}' " +
                        $"required by global milestone '{globalMilestone.Id}' was missing from the template collection.");
                }

                // Append the localized tracking dependency link directly to the execution milestone entity
                currentLocalSuccessor.AddInboundDependency(localPredecessor.Id, blueprintDependency.Type);
            }
        }

        // Return the fully wired collection of runtime milestone items
        return translationMatrix.Values;
    }
}
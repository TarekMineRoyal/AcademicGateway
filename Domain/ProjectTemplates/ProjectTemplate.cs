using System;
using System.Collections.Generic;
using System.Linq;
using AcademicGateway.Domain.Common;
using AcademicGateway.Domain.Common.Enums;
using AcademicGateway.Domain.ProjectTemplates.Exceptions;
using AcademicGateway.Domain.ProjectTemplates.Events;
using AcademicGateway.Domain.Providers;
using AcademicGateway.Domain.ProjectInstances.Enums;
using AcademicGateway.Domain.ProjectInstances;
using AcademicGateway.Domain.ProjectTemplates.Enums;
using AcademicGateway.Domain.ProjectInstances.Services;

namespace AcademicGateway.Domain.ProjectTemplates;

/// <summary>
/// Represents a reusable project blueprint proposed by a verified provider, which undergoes 
/// a collaborative review pipeline before becoming available to students. Serves as the 
/// Aggregate Root for the blueprint domain graph.
/// </summary>
public class ProjectTemplate : BaseEntity
{
    private readonly List<ProjectTemplateSkill> _projectTemplateSkills = new();
    private readonly List<GlobalMilestone> _globalMilestones = new();

    /// <summary>
    /// Gets the unique identifier for the project template.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the headline title of the project template.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the detailed description mapping requirements, scope, and objectives of the project.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the current lifecycle state of the template within the curation pipeline.
    /// </summary>
    public ProjectTemplateStatus Status { get; private set; }

    /// <summary>
    /// Gets the unique identifier of the creating provider account.
    /// </summary>
    public Guid ProviderId { get; private set; }

    /// <summary>
    /// Gets the operational feedback, change requests, or rejection reasons logged by the evaluating reviewer.
    /// </summary>
    public string? ReviewerFeedback { get; private set; }

    /// <summary>
    /// Gets the navigation property for the creating partner profile.
    /// </summary>
    public Provider Provider { get; private set; } = null!;

    /// <summary>
    /// Gets the read-only tracking collection of specific skill competencies mapped to this template.
    /// </summary>
    public IReadOnlyCollection<ProjectTemplateSkill> ProjectTemplateSkills => _projectTemplateSkills.AsReadOnly();

    /// <summary>
    /// Gets the read-only collection of milestones associated with this blueprint.
    /// </summary>
    public IReadOnlyCollection<GlobalMilestone> GlobalMilestones => _globalMilestones.AsReadOnly();

    /// <summary>
    /// EF Core constructor requirement. Prevents bypass of standard domain constraints during persistence hydration.
    /// </summary>
    private ProjectTemplate()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectTemplate"/> tracking model in Draft mode.
    /// </summary>
    /// <param name="title">The structural title of the project blueprint.</param>
    /// <param name="description">The core overview requirements text.</param>
    /// <param name="providerId">The identity tracker code mapping back to the owner profile.</param>
    /// <exception cref="InvalidTemplateDetailsException">Thrown when fundamental text parameters fail validation checks.</exception>
    public ProjectTemplate(string title, string description, Guid providerId)
    {
        if (providerId == Guid.Empty)
        {
            throw new InvalidTemplateDetailsException("Provider ID cannot be an empty Guid.");
        }

        Id = Guid.NewGuid();
        ProviderId = providerId;
        Status = ProjectTemplateStatus.Draft;

        UpdateDetails(title, description);

        // Append creation domain event message natively onto the tracking queue
        AddDomainEvent(new ProjectTemplateCreatedEvent(Id, ProviderId, Title));
    }

    /// <summary>
    /// Adds a new global milestone blueprint configuration to this template aggregate root.
    /// </summary>
    /// <param name="title">The title of the milestone.</param>
    /// <param name="description">The detailed milestone implementation instructions.</param>
    /// <param name="expectedEffortInHours">The effort-based scheduling estimation value.</param>
    /// <param name="deliverableType">The expected format constraint for submissions.</param>
    /// <exception cref="InvalidTemplateStatusException">Thrown if the template is already locked from edits.</exception>
    public void AddMilestone(string title, string description, decimal expectedEffortInHours, DeliverableType deliverableType)
    {
        if (Status == ProjectTemplateStatus.Approved || Status == ProjectTemplateStatus.Rejected)
        {
            throw new InvalidTemplateStatusException(Status, nameof(AddMilestone));
        }

        if (expectedEffortInHours <= 0)
        {
            throw new InvalidTemplateDetailsException("Expected effort must be greater than zero hours.");
        }

        var milestone = new GlobalMilestone(this.Id, title, description, expectedEffortInHours, deliverableType);
        _globalMilestones.Add(milestone);
    }

    /// <summary>
    /// Establishes a graph dependency constraint between two internal blueprint milestones with cyclic validation protection.
    /// </summary>
    /// <param name="successorId">The identifier of the milestone that depends on the predecessor.</param>
    /// <param name="predecessorId">The identifier of the milestone that must occur first.</param>
    /// <param name="type">The dependency constraint type rules (e.g. FinishToStart).</param>
    /// <exception cref="InvalidTemplateStatusException">Thrown if the aggregate state is immutable.</exception>
    /// <exception cref="InvalidOperationException">Thrown if a cyclic loop violation is detected.</exception>
    public void AddMilestoneDependency(Guid successorId, Guid predecessorId, DependencyType type)
    {
        if (Status == ProjectTemplateStatus.Approved || Status == ProjectTemplateStatus.Rejected)
        {
            throw new InvalidTemplateStatusException(Status, nameof(AddMilestoneDependency));
        }

        var successor = _globalMilestones.FirstOrDefault(m => m.Id == successorId);
        var predecessor = _globalMilestones.FirstOrDefault(m => m.Id == predecessorId);

        if (successor == null || predecessor == null)
        {
            throw new InvalidTemplateDetailsException("Both target milestones must exist within this template context.");
        }

        // Apply execution constraint locally to trace tentative graph mutations
        successor.AddPredecessor(predecessorId, type);

        // Verify that this new link does not introduce a cyclic graph trap
        EnsureGraphIsAcyclic();
    }

    /// <summary>
    /// Executes a continuous Directed Acyclic Graph (DAG) analysis using a Depth-First Search (DFS) topological loop tracking approach.
    /// </summary>
    private void EnsureGraphIsAcyclic()
    {
        // State tracker dictionary: Key = Milestone ID, Value: true = In Recursion Stack (Visiting), false = Fully Evaluated (Visited)
        var evaluationStates = new Dictionary<Guid, bool>();

        foreach (var milestone in _globalMilestones)
        {
            if (!evaluationStates.ContainsKey(milestone.Id))
            {
                if (DetectCycleDfs(milestone.Id, evaluationStates))
                {
                    throw new InvalidOperationException("Dependency rejected: Action introduces an invalid circular reference / DAG loop.");
                }
            }
        }
    }

    /// <summary>
    /// Internal recursive worker method executing structural path verification down the dependency chains.
    /// </summary>
    private bool DetectCycleDfs(Guid currentId, Dictionary<Guid, bool> states)
    {
        // Push onto the active evaluation recursion stack
        states[currentId] = true;

        var currentMilestone = _globalMilestones.First(m => m.Id == currentId);

        foreach (var dependency in currentMilestone.InboundDependencies)
        {
            if (states.TryGetValue(dependency.PredecessorId, out bool inStack))
            {
                if (inStack)
                {
                    return true; // Cycle detected: We ran into a node currently being processed up our stack!
                }
            }
            else
            {
                // Unvisited node found: follow its dependencies deeper into the graph
                if (DetectCycleDfs(dependency.PredecessorId, states))
                {
                    return true;
                }
            }
        }

        // Remove from current trace stack and mark as completely safe / evaluated
        states[currentId] = false;
        return false;
    }

    /// <summary>
    /// Updates the core textual specifications of the template. Accessible by providers during drafting/reworking, 
    /// or by reviewers during active evaluation.
    /// </summary>
    public void UpdateDetails(string newTitle, string newDescription)
    {
        if (Status == ProjectTemplateStatus.Approved || Status == ProjectTemplateStatus.Rejected)
        {
            throw new InvalidTemplateStatusException(Status, nameof(UpdateDetails));
        }

        if (string.IsNullOrWhiteSpace(newTitle))
        {
            throw new InvalidTemplateDetailsException("Project template title cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(newDescription))
        {
            throw new InvalidTemplateDetailsException("Project template description cannot be empty or whitespace.");
        }

        Title = newTitle.Trim();
        Description = newDescription.Trim();
    }

    /// <summary>
    /// Submits the drafted or revised project blueprint into the faculty review pool.
    /// </summary>
    public void SubmitForReview()
    {
        if (Status != ProjectTemplateStatus.Draft && Status != ProjectTemplateStatus.ChangesRequested)
        {
            throw new InvalidTemplateStatusException(Status, nameof(SubmitForReview));
        }

        Status = ProjectTemplateStatus.PendingReview;

        // Append workflow submission event to safely alert faculty reviewer pools
        AddDomainEvent(new ProjectTemplateSubmittedEvent(Id, ProviderId));
    }

    /// <summary>
    /// Direct Action: Approves the blueprint as-is, closing feedback loops and making it visible to the student platform.
    /// </summary>
    public void Approve()
    {
        if (Status != ProjectTemplateStatus.PendingReview)
        {
            throw new InvalidTemplateStatusException(Status, nameof(Approve));
        }

        Status = ProjectTemplateStatus.Approved;
        ReviewerFeedback = null;

        // Append terminal approval event to sync with student matching matrices and external contexts
        AddDomainEvent(new ProjectTemplateApprovedEvent(Id, ProviderId));
    }

    /// <summary>
    /// Collaborative Iteration Loop: Sends the template back to the provider requesting modifications.
    /// </summary>
    public void RequestChanges(string feedback)
    {
        if (Status != ProjectTemplateStatus.PendingReview)
        {
            throw new InvalidTemplateStatusException(Status, nameof(RequestChanges));
        }

        if (string.IsNullOrWhiteSpace(feedback))
        {
            throw new InvalidTemplateDetailsException("Feedback instructions must be provided to guide the provider's corrections.");
        }

        Status = ProjectTemplateStatus.ChangesRequested;
        ReviewerFeedback = feedback.Trim();

        // Append state modification request event containing the precise auditor feedback payload
        AddDomainEvent(new ProjectTemplateChangesRequestedEvent(Id, ProviderId, ReviewerFeedback));
    }

    /// <summary>
    /// Collaborative Iteration Loop: Allows a reviewer to refine the text parameters directly 
    /// while pushing the blueprint into a confirmation hold for the provider's explicit sign-off.
    /// </summary>
    public void ProposeReviewerChanges(string adjustedTitle, string adjustedDescription)
    {
        if (Status != ProjectTemplateStatus.PendingReview)
        {
            throw new InvalidTemplateStatusException(Status, nameof(ProposeReviewerChanges));
        }

        // Apply edits directly to the entity state using internal methods
        UpdateDetails(adjustedTitle, adjustedDescription);

        // Shift next-action dependency over to the provider
        Status = ProjectTemplateStatus.PendingProviderAcceptance;
        ReviewerFeedback = "Reviewer has modified details. Awaiting provider confirmation.";

        // Append hand-off event to notify the corporate provider that modifications require their confirmation
        AddDomainEvent(new ProjectTemplateReviewerChangesProposedEvent(Id, ProviderId));
    }

    /// <summary>
    /// Collaborative Iteration Loop Sign-Off: Executed by the Provider to accept the reviewer's 
    /// proposed alterations, instantly certifying the template into active service.
    /// </summary>
    public void ProviderAcceptProposedChanges()
    {
        if (Status != ProjectTemplateStatus.PendingProviderAcceptance)
        {
            throw new InvalidTemplateStatusException(Status, nameof(ProviderAcceptProposedChanges));
        }

        Status = ProjectTemplateStatus.Approved;
        ReviewerFeedback = null;

        // Append terminal approval event since provider validation elevates this blueprint directly to live service
        AddDomainEvent(new ProjectTemplateApprovedEvent(Id, ProviderId));
    }

    /// <summary>
    /// Collaborative Iteration Loop Sign-Off: Executed by the Provider to reject the reviewer's 
    /// proposed alterations, reverting the template back to a Draft layout for manual adjustments.
    /// </summary>
    public void ProviderRejectProposedChanges()
    {
        if (Status != ProjectTemplateStatus.PendingProviderAcceptance)
        {
            throw new InvalidTemplateStatusException(Status, nameof(ProviderRejectProposedChanges));
        }

        Status = ProjectTemplateStatus.Draft;
        ReviewerFeedback = "Provider declined reviewer alterations. Reverted back to draft layout.";

        // Append reversion event to track collaborative iteration conflicts or alert administrative staff
        AddDomainEvent(new ProjectTemplateReviewerChangesRejectedEvent(Id, ProviderId));
    }

    /// <summary>
    /// Final Action: Permanently denies the submission, locking it from subsequent corrections or resubmissions.
    /// </summary>
    public void RejectPermanently(string reason)
    {
        if (Status != ProjectTemplateStatus.PendingReview)
        {
            throw new InvalidTemplateStatusException(Status, nameof(RejectPermanently));
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidTemplateDetailsException("A strict justification reason must be logged for permanent rejection.");
        }

        Status = ProjectTemplateStatus.Rejected;
        ReviewerFeedback = reason.Trim();

        // Append terminal rejection event documenting strict administrative refusal
        AddDomainEvent(new ProjectTemplateRejectedPermanentlyEvent(Id, ProviderId, ReviewerFeedback));
    }

    /// <summary>
    /// Maps a required tracking skill competency to this template matrix.
    /// </summary>
    public void AddSkill(Guid skillId)
    {
        if (skillId == Guid.Empty)
        {
            throw new InvalidTemplateDetailsException("Skill ID cannot be an empty Guid.");
        }

        if (_projectTemplateSkills.Any(pts => pts.SkillId == skillId))
        {
            return; // Skill mapping already established
        }

        if (_projectTemplateSkills.Count >= 10)
        {
            throw new InvalidTemplateDetailsException("A single project template cannot require more than 10 technical skills.");
        }

        _projectTemplateSkills.Add(new ProjectTemplateSkill(Id, skillId));
    }

    /// <summary>
    /// Removes an existing tracking skill requirements link from this template matrix.
    /// </summary>
    public void RemoveSkill(Guid skillId)
    {
        var skillMapping = _projectTemplateSkills.FirstOrDefault(pts => pts.SkillId == skillId);
        if (skillMapping != null)
        {
            _projectTemplateSkills.Remove(skillMapping);
        }
    }

    /// <summary>
    /// Factory Method (Prototype Pattern): Instantiates a brand new, isolated live project workspace 
    /// aggregate root for a student based on this approved template's current point-in-time state snapshot.
    /// Uses double-dispatch to safely construct the internal milestone graph within the domain assembly boundary.
    /// </summary>
    /// <param name="studentId">The unique tracking identifier of the student initiating the project.</param>
    /// <param name="createdAt">The deterministic timestamp marking workspace initialization.</param>
    /// <param name="milestoneFactory">The domain factory service responsible for re-mapping graph identifier topologies.</param>
    /// <param name="initialRequestedProfessorId">Optional supervisor ID if requesting mentoring at startup.</param>
    /// <returns>A fully hydrated, structurally complete <see cref="ProjectInstance"/> aggregate root ready for persistence.</returns>
    /// <exception cref="InvalidTemplateStatusException">Thrown if an attempt is made to instantiate a template that is not Approved.</exception>
    public ProjectInstance Instantiate(
        Guid studentId,
        DateTime createdAt,
        LocalMilestoneFactory milestoneFactory,
        Guid? initialRequestedProfessorId = null)
    {
        // Guard Invariant: Students can only spin up workspaces from fully verified and approved blueprints
        if (Status != ProjectTemplateStatus.Approved)
        {
            throw new InvalidTemplateStatusException(Status, nameof(Instantiate));
        }

        if (studentId == Guid.Empty)
        {
            throw new InvalidTemplateDetailsException("Student ID cannot be an empty Guid when instantiating a project.");
        }

        if (milestoneFactory == null)
        {
            throw new ArgumentNullException(nameof(milestoneFactory), "The local milestone snapshot factory service is required.");
        }

        // Determine the initial lifecycle track based on whether a professor was chosen at startup
        var initialStatus = initialRequestedProfessorId.HasValue
            ? ProjectInstanceStatus.AwaitingSupervision
            : ProjectInstanceStatus.Active;

        // Isolate technical skill IDs from the template join collection to copy into the snapshot
        var skillIdsSnapshot = _projectTemplateSkills.Select(pts => pts.SkillId);

        // 1. Manufacture the new aggregate root shell instance
        var projectInstance = new ProjectInstance(
            studentId,
            Id,
            ProviderId,
            Title,
            Description,
            initialStatus,
            createdAt,
            initialRequestedProfessorId,
            skillIdsSnapshot
        );

        // 2. Map the entirely detached execution milestone collections across fresh identifier spaces.
        // Because this code executes within the Domain project assembly, we can safely invoke the internal methods.
        var localMilestonesSnapshot = milestoneFactory.CreateLocalMilestonesSnapshot(
            projectInstance.Id,
            this._globalMilestones);

        // 3. Populate the generated milestone collection into the new aggregate root boundary safely
        projectInstance.SeedClonedMilestones(localMilestonesSnapshot);

        // Return the fully complete, structurally consistent aggregate root
        return projectInstance;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using AcademicGateway.Domain.Common;
using AcademicGateway.Domain.ProjectTemplates.Enums;
using AcademicGateway.Domain.ProjectTemplates.Exceptions;
using AcademicGateway.Domain.ProjectTemplates.Events;
using AcademicGateway.Domain.Providers;

namespace AcademicGateway.Domain.ProjectTemplates;

/// <summary>
/// Represents a reusable project blueprint proposed by a verified provider, which undergoes 
/// a collaborative review pipeline before becoming available to students.
/// </summary>
public class ProjectTemplate : BaseEntity
{
    private readonly List<ProjectTemplateSkill> _projectTemplateSkills = new();

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
    /// Updates the core textual specifications of the template. Accessible by providers during drafting/reworking, 
    /// or by reviewers during active evaluation.
    /// </summary>
    /// <param name="newTitle">The updated headline title.</param>
    /// <param name="newDescription">The updated execution details description.</param>
    /// <exception cref="InvalidTemplateDetailsException">Thrown if any parameter text criteria checks fail.</exception>
    /// <exception cref="InvalidTemplateStatusException">Thrown if edits are performed on an immutable state.</exception>
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
    /// <exception cref="InvalidTemplateStatusException">Thrown if executed outside the Draft or ChangesRequested state contexts.</exception>
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
    /// <exception cref="InvalidTemplateStatusException">Thrown if executed outside an active review evaluation context.</exception>
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
    /// <param name="feedback">Specific instructions detailing the required corrections.</param>
    /// <exception cref="InvalidTemplateDetailsException">Thrown if feedback commentary text is invalid.</exception>
    /// <exception cref="InvalidTemplateStatusException">Thrown if executed outside a PendingReview context.</exception>
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
    /// <param name="adjustedTitle">The corrected or optimized title text.</param>
    /// <param name="adjustedDescription">The corrected or optimized description content.</param>
    /// <exception cref="InvalidTemplateStatusException">Thrown if executed outside a PendingReview context.</exception>
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
    /// <exception cref="InvalidTemplateStatusException">Thrown if executed outside a PendingProviderAcceptance context.</exception>
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
    /// <exception cref="InvalidTemplateStatusException">Thrown if executed outside a PendingProviderAcceptance context.</exception>
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
    /// <param name="reason">The administrative reason justifying a hard denial.</param>
    /// <exception cref="InvalidTemplateDetailsException">Thrown if feedback commentary text is invalid.</exception>
    /// <exception cref="InvalidTemplateStatusException">Thrown if executed outside a PendingReview context.</exception>
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
    /// <param name="skillId">The target skill unique identifier.</param>
    /// <exception cref="InvalidTemplateDetailsException">Thrown if the provided identifier is empty or collection limit invariants are broken.</exception>
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

        // Deep Domain Guard: Ensures aggregate root protects its layout boundaries independently of application validators
        if (_projectTemplateSkills.Count >= 10)
        {
            throw new InvalidTemplateDetailsException("A single project template cannot require more than 10 technical skills.");
        }

        _projectTemplateSkills.Add(new ProjectTemplateSkill(Id, skillId));
    }

    /// <summary>
    /// Removes an existing tracking skill requirements link from this template matrix.
    /// </summary>
    /// <param name="skillId">The target skill unique identifier to isolate and drop.</param>
    public void RemoveSkill(Guid skillId)
    {
        var skillMapping = _projectTemplateSkills.FirstOrDefault(pts => pts.SkillId == skillId);
        if (skillMapping != null)
        {
            _projectTemplateSkills.Remove(skillMapping);
        }
    }
}
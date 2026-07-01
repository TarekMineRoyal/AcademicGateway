using System;
using System.Collections.Generic;
using System.Linq;
using Domain.ProjectTemplates.Enums;
using Domain.Providers;

namespace Domain.ProjectTemplates;

/// <summary>
/// Represents a reusable project blueprint proposed by a verified provider, which undergoes 
/// a collaborative review pipeline before becoming available to students.
/// </summary>
public class ProjectTemplate
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
    /// Gets the unique string identifier of the creating provider account.
    /// </summary>
    public string ProviderId { get; private set; } = string.Empty;

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
    /// <exception cref="ArgumentException">Thrown when any text parameter validation checks fail.</exception>
    public ProjectTemplate(string title, string description, string providerId)
    {
        if (string.IsNullOrWhiteSpace(providerId))
        {
            throw new ArgumentException("Provider ID cannot be empty or whitespace.", nameof(providerId));
        }

        Id = Guid.NewGuid();
        ProviderId = providerId.Trim();
        Status = ProjectTemplateStatus.Draft;

        UpdateDetails(title, description);
    }

    /// <summary>
    /// Updates the core textual specifications of the template. Accessible by providers during drafting/reworking, 
    /// or by reviewers during active evaluation.
    /// </summary>
    /// <param name="newTitle">The updated headline title.</param>
    /// <param name="newDescription">The updated execution details description.</param>
    /// <exception cref="ArgumentException">Thrown if any parameter text criteria checks fail.</exception>
    /// <exception cref="InvalidOperationException">Thrown if edits are performed on an immutable state.</exception>
    public void UpdateDetails(string newTitle, string newDescription)
    {
        if (Status == ProjectTemplateStatus.Approved || Status == ProjectTemplateStatus.Rejected)
        {
            throw new InvalidOperationException($"Project templates cannot be modified while in the {Status} state.");
        }

        if (string.IsNullOrWhiteSpace(newTitle))
        {
            throw new ArgumentException("Project template title cannot be empty or whitespace.", nameof(newTitle));
        }

        if (string.IsNullOrWhiteSpace(newDescription))
        {
            throw new ArgumentException("Project template description cannot be empty or whitespace.", nameof(newDescription));
        }

        Title = newTitle.Trim();
        Description = newDescription.Trim();
    }

    /// <summary>
    /// Submits the drafted or revised project blueprint into the faculty review pool.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if executed outside the Draft or ChangesRequested state contexts.</exception>
    public void SubmitForReview()
    {
        if (Status != ProjectTemplateStatus.Draft && Status != ProjectTemplateStatus.ChangesRequested)
        {
            throw new InvalidOperationException("Only templates in Draft or ChangesRequested status can be submitted for review.");
        }

        Status = ProjectTemplateStatus.PendingReview;
    }

    /// <summary>
    /// Direct Action: Approves the blueprint as-is, closing feedback loops and making it visible to the student platform.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if executed outside an active review evaluation context.</exception>
    public void Approve()
    {
        if (Status != ProjectTemplateStatus.PendingReview)
        {
            throw new InvalidOperationException("Only templates currently pending review can be approved directly.");
        }

        Status = ProjectTemplateStatus.Approved;
        ReviewerFeedback = null;
    }

    /// <summary>
    /// Collaborative Iteration Loop: Sends the template back to the provider requesting modifications.
    /// </summary>
    /// <param name="feedback">Specific instructions detailing the required corrections.</param>
    /// <exception cref="ArgumentException">Thrown if feedback commentary text is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown if executed outside a PendingReview context.</exception>
    public void RequestChanges(string feedback)
    {
        if (Status != ProjectTemplateStatus.PendingReview)
        {
            throw new InvalidOperationException("Changes can only be requested on templates currently pending review.");
        }

        if (string.IsNullOrWhiteSpace(feedback))
        {
            throw new ArgumentException("Feedback instructions must be provided to guide the provider's corrections.", nameof(feedback));
        }

        Status = ProjectTemplateStatus.ChangesRequested;
        ReviewerFeedback = feedback.Trim();
    }

    /// <summary>
    /// Collaborative Iteration Loop: Allows a reviewer to refine the text parameters directly 
    /// while pushing the blueprint into a confirmation hold for the provider's explicit sign-off.
    /// </summary>
    /// <param name="adjustedTitle">The corrected or optimized title text.</param>
    /// <param name="adjustedDescription">The corrected or optimized description content.</param>
    /// <exception cref="InvalidOperationException">Thrown if executed outside a PendingReview context.</exception>
    public void ProposeReviewerChanges(string adjustedTitle, string adjustedDescription)
    {
        if (Status != ProjectTemplateStatus.PendingReview)
        {
            throw new InvalidOperationException("Reviewer updates can only be proposed on templates currently pending review.");
        }

        // Apply edits directly to the entity state
        UpdateDetails(adjustedTitle, adjustedDescription);

        // Shift next-action dependency over to the provider
        Status = ProjectTemplateStatus.PendingProviderAcceptance;
        ReviewerFeedback = "Reviewer has modified details. Awaiting provider confirmation.";
    }

    /// <summary>
    /// Collaborative Iteration Loop Sign-Off (Scenario 3): Executed by the Provider to accept the reviewer's 
    /// proposed alterations, instantly certifying the template into active service.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if executed outside a PendingProviderAcceptance context.</exception>
    public void ProviderAcceptProposedChanges()
    {
        if (Status != ProjectTemplateStatus.PendingProviderAcceptance)
        {
            throw new InvalidOperationException("There are no proposed reviewer modifications to accept on this template.");
        }

        Status = ProjectTemplateStatus.Approved;
        ReviewerFeedback = null;
    }

    /// <summary>
    /// Collaborative Iteration Loop Sign-Off (Scenario 3): Executed by the Provider to reject the reviewer's 
    /// alterations, reverting the template back to a Draft layout for manual adjustments.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if executed outside a PendingProviderAcceptance context.</exception>
    public void ProviderRejectProposedChanges()
    {
        if (Status != ProjectTemplateStatus.PendingProviderAcceptance)
        {
            throw new InvalidOperationException("There are no proposed reviewer modifications to reject.");
        }

        Status = ProjectTemplateStatus.Draft;
        ReviewerFeedback = "Provider declined reviewer alterations. Reverted back to draft layout.";
    }

    /// <summary>
    /// Final Action: Permanently denies the submission, locking it from subsequent corrections or resubmissions.
    /// </summary>
    /// <param name="reason">The administrative reason justifying a hard denial.</param>
    /// <exception cref="ArgumentException">Thrown if feedback commentary text is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown if executed outside a PendingReview context.</exception>
    public void RejectPermanently(string reason)
    {
        if (Status != ProjectTemplateStatus.PendingReview)
        {
            throw new InvalidOperationException("Only templates currently pending review can be permanently rejected.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("A strict justification reason must be logged for permanent rejection.", nameof(reason));
        }

        Status = ProjectTemplateStatus.Rejected;
        ReviewerFeedback = reason.Trim();
    }

    /// <summary>
    /// Maps a required tracking skill competency to this template matrix.
    /// </summary>
    /// <param name="skillId">The target skill unique identifier.</param>
    /// <exception cref="ArgumentException">Thrown if the provided identifier is empty.</exception>
    public void AddSkill(Guid skillId)
    {
        if (skillId == Guid.Empty)
        {
            throw new ArgumentException("Skill ID cannot be an empty Guid.", nameof(skillId));
        }

        if (_projectTemplateSkills.Any(pts => pts.SkillId == skillId))
        {
            return; // Skill mapping already established
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
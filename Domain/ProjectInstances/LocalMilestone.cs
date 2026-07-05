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
/// Holds scheduling timelines, state changes, polymorphic submission strings, comments, and evaluation metrics.
/// </summary>
public class LocalMilestone : BaseEntity
{
    private readonly List<LocalMilestoneDependency> _inboundDependencies = new();
    private readonly List<MilestoneComment> _comments = new();

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
    /// Gets the format category constraint mapped for student submission handling.
    /// </summary>
    public DeliverableType RequiredDeliverableType { get; private set; }

    /// <summary>
    /// Gets the current state within the individual task execution state machine.
    /// </summary>
    public LocalMilestoneStatus Status { get; internal set; }

    /// <summary>
    /// Gets the student-assigned start date for this execution leg. Nullable until scheduled per Rule 3.
    /// </summary>
    public DateTime? ScheduledStartDate { get; internal set; }

    /// <summary>
    /// Gets the student-assigned deadline target for this execution leg. Nullable until scheduled per Rule 3.
    /// </summary>
    public DateTime? ScheduledEndDate { get; internal set; }

    /// <summary>
    /// Gets the polymorphic raw submission content payload text (e.g. URL string, text summary, file token locator).
    /// </summary>
    public string? SubmissionPayload { get; internal set; }

    /// <summary>
    /// Gets the exact tracking timestamp when the student completed the deliverable push.
    /// </summary>
    public DateTime? SubmittedAt { get; private set; }

    /// <summary>
    /// Gets the final numerical score value awarded during professor evaluation.
    /// </summary>
    public decimal? Grade { get; private set; }

    /// <summary>
    /// Gets the formal evaluation audit feedback commentary logged by the grading mentor.
    /// </summary>
    public string? EvaluationFeedback { get; private set; }

    /// <summary>
    /// Gets the timestamp tracking when evaluation calculations were certified.
    /// </summary>
    public DateTime? GradedAt { get; private set; }

    /// <summary>
    /// Exposes incoming dependency boundaries as an encapsulated read-only sequence structure.
    /// </summary>
    public IReadOnlyCollection<LocalMilestoneDependency> InboundDependencies => _inboundDependencies.AsReadOnly();

    /// <summary>
    /// Exposes timestamped cross-role conversation commentary logged within this milestone workspace channel.
    /// </summary>
    public IReadOnlyCollection<MilestoneComment> Comments => _comments.AsReadOnly();

    /// <summary>
    /// Parameterless constructor required by EF Core for persistence materialization loops.
    /// </summary>
    private LocalMilestone()
    {
        TitleSnapshot = null!;
        DescriptionSnapshot = null!;
    }

    /// <summary>
    /// Factory-scoped instantiation constructor designed to generate an isolated execution milestone.
    /// </summary>
    public LocalMilestone(
        Guid projectInstanceId,
        string titleSnapshot,
        string descriptionSnapshot,
        decimal expectedEffortInHours,
        DeliverableType requiredDeliverableType)
    {
        Id = Guid.NewGuid();
        ProjectInstanceId = projectInstanceId;
        TitleSnapshot = titleSnapshot.Trim();
        DescriptionSnapshot = descriptionSnapshot.Trim();
        ExpectedEffortInHours = expectedEffortInHours;
        RequiredDeliverableType = requiredDeliverableType;
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
    /// Processes a student deliverable submission, validating formatting rules against polymorphic constraints.
    /// </summary>
    internal void SubmitDeliverable(string payload, DateTime utcNow)
    {
        if (Status == LocalMilestoneStatus.Graded)
        {
            throw new InvalidOperationException($"Submission Denied: Milestone '{TitleSnapshot}' has already been graded and closed out.");
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new InvalidOperationException("Submission Denied: The deliverable payload data cannot be empty or whitespace.");
        }

        var cleanedPayload = payload.Trim();

        switch (RequiredDeliverableType)
        {
            case DeliverableType.Url:
                if (!cleanedPayload.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !cleanedPayload.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Submission Format Error: Milestone '{TitleSnapshot}' requires a valid web repository link destination " +
                        $"(must begin with 'http://' or 'https://'). Provided: '{cleanedPayload}'");
                }
                break;

            case DeliverableType.File:
                if (cleanedPayload.Length < 5)
                {
                    throw new InvalidOperationException("Submission Format Error: The file storage identifier payload appears invalid or corrupted.");
                }
                break;

            case DeliverableType.Text:
                if (cleanedPayload.Length > 4000)
                {
                    throw new InvalidOperationException("Submission Format Error: Text entry summary exceeds maximum length limit of 4000 characters.");
                }
                break;

            case DeliverableType.None:
            default:
                break;
        }

        SubmissionPayload = cleanedPayload;
        SubmittedAt = utcNow;
        Status = LocalMilestoneStatus.Submitted;
    }

    /// <summary>
    /// Registers an academic evaluation score and commentary from an authorized supervisor,
    /// dynamically executing validation logic against the injected domain grading strategy.
    /// </summary>
    internal void EvaluateSubmission(decimal grade, string? feedback, IGradingStrategy gradingStrategy, DateTime utcNow)
    {
        if (gradingStrategy == null)
        {
            throw new ArgumentNullException(nameof(gradingStrategy), "An evaluation execution requires a valid grading strategy instance.");
        }

        if (Status != LocalMilestoneStatus.Submitted)
        {
            throw new InvalidOperationException(
                $"Evaluation Denied: Milestone '{TitleSnapshot}' is currently '{Status}'. " +
                "An item must be explicitly marked as 'Submitted' before grading operations can be processed.");
        }

        if (!gradingStrategy.IsValidMilestoneGrade(grade))
        {
            throw new InvalidOperationException(
                $"Grading Strategy Invariant Violation: The proposed score value '{grade}' is mathematically " +
                $"invalid for the target evaluation layout: '{gradingStrategy.StrategyName}'.");
        }

        Grade = grade;
        EvaluationFeedback = string.IsNullOrWhiteSpace(feedback) ? null : feedback.Trim();
        GradedAt = utcNow;
        Status = LocalMilestoneStatus.Graded;
    }

    // =========================================================================
    // CROSS-ROLE MILESTONE DISCUSSION ENGINE MUTATOR
    // =========================================================================

    /// <summary>
    /// Appends a new immutable collaboration commentary entry log directly onto this milestone tracking lane.
    /// </summary>
    /// <param name="authorId">The unique account identifier code linking back to the posting platform user.</param>
    /// <param name="authorIdentitySnapshot">The string description defining the functional role authority of the poster.</param>
    /// <param name="content">The raw message textual copy containing details, questions, or clarification instructions.</param>
    /// <param name="utcNow">The current synchronized system timestamp execution coordinate.</param>
    internal void AddComment(Guid authorId, string authorIdentitySnapshot, string content, DateTime utcNow)
    {
        // Instantiation encapsulates and executes character length and empty-space checking guards automatically
        var comment = new MilestoneComment(this.Id, authorId, authorIdentitySnapshot, content, utcNow);
        _comments.Add(comment);
    }
}
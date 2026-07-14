using System;
using AcademicGateway.Domain.Common;
using AcademicGateway.Domain.Common.Enums;
using AcademicGateway.Domain.ProjectInstances.Enums;
using AcademicGateway.Domain.ProjectInstances.Grading;

namespace AcademicGateway.Domain.ProjectInstances;

/// <summary>
/// Represents an isolated, active runtime task managed inside a local milestone container.
/// Holds individual execution states, polymorphic submission payloads, evaluation scores, and metrics.
/// </summary>
public class LocalTask : BaseEntity
{
    /// <summary>
    /// Gets the unique tracking identifier for this runtime task instance.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the unique identifier of the parent LocalMilestone container.
    /// </summary>
    public Guid LocalMilestoneId { get; private set; }

    /// <summary>
    /// Gets the static descriptive snapshot title copied from the source blueprint definition.
    /// </summary>
    public string TitleSnapshot { get; private set; }

    /// <summary>
    /// Gets the static descriptive instructions copy fetched from the template.
    /// </summary>
    public string DescriptionSnapshot { get; private set; }

    /// <summary>
    /// Gets the operational progress weight assigned to this task relative to its parent milestone.
    /// </summary>
    public decimal Weight { get; private set; }

    /// <summary>
    /// Gets the format category constraint mapped for student submission handling.
    /// </summary>
    public DeliverableType RequiredDeliverableType { get; private set; }

    /// <summary>
    /// Gets the current state within the individual task execution state machine.
    /// </summary>
    public LocalTaskStatus Status { get; internal set; }

    /// <summary>
    /// Gets the polymorphic raw submission content payload text (e.g., URL string, text summary, file token locator).
    /// </summary>
    public string? SubmissionPayload { get; internal set; }

    /// <summary>
    /// Gets the exact tracking timestamp when the student completed the deliverable push.
    /// </summary>
    public DateTime? SubmittedAt { get; private set; }

    /// <summary>
    /// Gets the final numerical score value awarded during evaluation.
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
    /// Parameterless constructor required by EF Core for persistence materialization loops.
    /// </summary>
    private LocalTask()
    {
        TitleSnapshot = null!;
        DescriptionSnapshot = null!;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalTask"/> child domain entity.
    /// Constructor is marked internal to force instantiation solely through the domain boundary.
    /// </summary>
    /// <param name="localMilestoneId">The tracking identifier of the parent milestone container.</param>
    /// <param name="titleSnapshot">The static title text snapshotted from the blueprint definition.</param>
    /// <param name="descriptionSnapshot">The background details snapshotted from the blueprint definition.</param>
    /// <param name="weight">The operational progress weight assigned to this task node configuration.</param>
    /// <param name="requiredDeliverableType">The explicit deliverable tracking submission constraint rule token.</param>
    internal LocalTask(
        Guid localMilestoneId,
        string titleSnapshot,
        string descriptionSnapshot,
        decimal weight,
        DeliverableType requiredDeliverableType)
    {
        Id = Guid.NewGuid();
        LocalMilestoneId = localMilestoneId;
        TitleSnapshot = titleSnapshot.Trim();
        DescriptionSnapshot = descriptionSnapshot.Trim();
        Weight = weight;
        RequiredDeliverableType = requiredDeliverableType;
        Status = LocalTaskStatus.NotStarted;
    }

    /// <summary>
    /// Processes a student deliverable submission, validating formatting rules against polymorphic constraints.
    /// </summary>
    /// <param name="payload">The raw message data or submission reference.</param>
    /// <param name="utcNow">The current synchronized system timestamp execution coordinate.</param>
    /// <exception cref="InvalidOperationException">Thrown if the task is already graded or formatting parameters are broken.</exception>
    internal void SubmitDeliverable(string payload, DateTime utcNow)
    {
        if (Status == LocalTaskStatus.Graded)
        {
            throw new InvalidOperationException($"Submission Denied: Task '{TitleSnapshot}' has already been graded and closed out.");
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
                        $"Submission Format Error: Task '{TitleSnapshot}' requires a valid web repository link destination " +
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
        Status = LocalTaskStatus.Submitted;
    }

    /// <summary>
    /// Registers an academic evaluation score and commentary from an authorized supervisor,
    /// dynamically executing validation logic against the injected domain grading strategy.
    /// </summary>
    /// <param name="grade">The numerical score value awarded to the deliverable push.</param>
    /// <param name="feedback">Optional critique commentaries logged by the evaluator.</param>
    /// <param name="gradingStrategy">The concrete domain strategy algorithm governing evaluation rules.</param>
    /// <param name="utcNow">The current synchronized system timestamp execution coordinate.</param>
    /// <exception cref="ArgumentNullException">Thrown if the provided strategy instance is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the task state or score validation bounds are broken.</exception>
    internal void EvaluateSubmission(decimal grade, string? feedback, IGradingStrategy gradingStrategy, DateTime utcNow)
    {
        if (gradingStrategy == null)
        {
            throw new ArgumentNullException(nameof(gradingStrategy), "An evaluation execution requires a valid grading strategy instance.");
        }

        if (Status != LocalTaskStatus.Submitted)
        {
            throw new InvalidOperationException(
                $"Evaluation Denied: Task '{TitleSnapshot}' is currently '{Status}'. " +
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
        Status = LocalTaskStatus.Graded;
    }
}
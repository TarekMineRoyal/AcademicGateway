using System;
using AcademicGateway.Domain.Common;
using AcademicGateway.Domain.ProjectInstances.Enums;
using AcademicGateway.Domain.Professors;

namespace AcademicGateway.Domain.ProjectInstances;

/// <summary>
/// Represents a stateful academic matchmaking request sent from a student to a professor 
/// asking for formal project supervision.
/// </summary>
public class SupervisionRequest : BaseEntity
{
    /// <summary>
    /// Gets the unique identifier for the supervision request.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the parent project instance ID associated with this invite.
    /// </summary>
    public Guid ProjectInstanceId { get; private set; }

    /// <summary>
    /// Gets the unique identifier of the targeted faculty member.
    /// </summary>
    public Guid ProfessorId { get; private set; }

    /// <summary>
    /// Gets the motivational text, goals, or proposal pitch written by the student.
    /// </summary>
    public string PitchText { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the current status of the matching proposal.
    /// </summary>
    public SupervisionRequestStatus Status { get; internal set; }

    /// <summary>
    /// Gets the explanatory notes or feedback provided by the professor upon refusal.
    /// </summary>
    public string? RejectionReason { get; internal set; }

    /// <summary>
    /// Gets the precise timestamp recording when this application was issued.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Gets the timestamp specifying when the professor accepted or declined this invitation.
    /// </summary>
    public DateTime? ReviewedAt { get; private set; }

    /// <summary>
    /// Gets the navigation property back to the containing project workspace workspace.
    /// </summary>
    public ProjectInstance ProjectInstance { get; private set; } = null!;

    /// <summary>
    /// Gets the navigation property tracking the targeted professor's profile.
    /// </summary>
    public Professor Professor { get; private set; } = null!;

    /// <summary>
    /// EF Core constructor requirement for database hydration.
    /// </summary>
    private SupervisionRequest()
    {
    }

    /// <summary>
    /// Initializes a new instance of an academic supervision match tracking record.
    /// </summary>
    internal SupervisionRequest(Guid projectInstanceId, Guid professorId, string pitchText, DateTime createdAt)
    {
        Id = Guid.NewGuid();
        ProjectInstanceId = projectInstanceId;
        ProfessorId = professorId;
        PitchText = pitchText.Trim();
        Status = SupervisionRequestStatus.Pending;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Updates the review status and records the deterministic execution timestamp.
    /// </summary>
    public void RecordReview(SupervisionRequestStatus finalStatus, string? rejectionReason, DateTime reviewedAt)
    {
        Status = finalStatus;
        RejectionReason = rejectionReason;
        ReviewedAt = reviewedAt;
    }
}
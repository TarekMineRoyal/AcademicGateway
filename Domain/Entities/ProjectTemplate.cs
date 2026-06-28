using AcademicGateway.Domain.Enums;

namespace AcademicGateway.Domain.Entities;

public class ProjectTemplate
{
    public Guid Id { get; private set; }
    public string ProviderId { get; private set; } = string.Empty; // Aligned to string for Identity matching
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int ExpectedDurationWeeks { get; private set; }
    public ProjectTemplateStatus Status { get; private set; }
    public Guid? ApprovedById { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public Provider Provider { get; private set; } = null!;
    public Reviewer? ApprovedBy { get; private set; }
    public ICollection<ProjectTemplateSkill> TemplateSkills { get; private set; } = new List<ProjectTemplateSkill>();

    private ProjectTemplate() { }

    public ProjectTemplate(string providerId, string title, string description, int expectedDurationWeeks)
    {
        Id = Guid.NewGuid();
        ProviderId = providerId;
        Title = title;
        Description = description;
        ExpectedDurationWeeks = expectedDurationWeeks;
        Status = ProjectTemplateStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    // State Transition Guards
    public void SubmitForReview()
    {
        if (Status != ProjectTemplateStatus.Draft)
            throw new InvalidOperationException("Only draft templates can be submitted for review.");

        Status = ProjectTemplateStatus.PendingReview;
    }

    public void Approve(Guid reviewerId)
    {
        if (Status != ProjectTemplateStatus.PendingReview)
            throw new InvalidOperationException("Only pending templates can be approved.");

        Status = ProjectTemplateStatus.Approved;
        ApprovedById = reviewerId;
        RejectionReason = null;
    }

    public void Reject(Guid reviewerId, string reason)
    {
        if (Status != ProjectTemplateStatus.PendingReview)
            throw new InvalidOperationException("Only pending templates can be rejected.");

        Status = ProjectTemplateStatus.Rejected;
        ApprovedById = reviewerId;
        RejectionReason = reason;
    }

    public void Archive()
    {
        Status = ProjectTemplateStatus.Archived;
    }
}
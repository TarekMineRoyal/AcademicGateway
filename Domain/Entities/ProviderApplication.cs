using AcademicGateway.Domain.Enums;

namespace AcademicGateway.Domain.Entities;

public class ProviderApplication
{
    public Guid Id { get; private set; }
    public string ProviderId { get; private set; } = string.Empty; // Fixed: string type to match Provider UserId
    public string CompanyDetails { get; private set; } = string.Empty;
    public string VerificationDocumentsUrl { get; private set; } = string.Empty;
    public ProviderApplicationStatus Status { get; private set; }
    public Guid? ReviewedById { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ReviewedAt { get; private set; }

    // Navigation properties
    public Provider Provider { get; private set; } = null!; // Fixed: Valid C# syntax
    public Reviewer? ReviewedBy { get; private set; }

    private ProviderApplication() { }

    public ProviderApplication(string providerId, string companyDetails, string verificationDocumentsUrl)
    {
        Id = Guid.NewGuid();
        ProviderId = providerId;
        CompanyDetails = companyDetails;
        VerificationDocumentsUrl = verificationDocumentsUrl;
        Status = ProviderApplicationStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    // State Transition Guards
    public void SubmitForReview()
    {
        if (Status != ProviderApplicationStatus.Draft)
            throw new InvalidOperationException("Only draft applications can be submitted for review.");

        Status = ProviderApplicationStatus.PendingReview;
    }

    public void Approve(Guid reviewerId)
    {
        if (Status != ProviderApplicationStatus.PendingReview)
            throw new InvalidOperationException("Only pending applications can be approved.");

        Status = ProviderApplicationStatus.Approved;
        ReviewedById = reviewerId;
        ReviewedAt = DateTime.UtcNow;
        RejectionReason = null;
    }

    public void Reject(Guid reviewerId, string reason)
    {
        if (Status != ProviderApplicationStatus.PendingReview)
            throw new InvalidOperationException("Only pending applications can be rejected.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A rejection reason must be provided.", nameof(reason));

        Status = ProviderApplicationStatus.Rejected;
        ReviewedById = reviewerId;
        ReviewedAt = DateTime.UtcNow;
        RejectionReason = reason;
    }
}
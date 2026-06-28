namespace AcademicGateway.Domain.Entities;

public class TechSupportAccount
{
    public Guid Id { get; private set; }
    public string ProviderId { get; private set; } = string.Empty; // Aligned to string for Identity matching
    public string IdentityUserId { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    // Navigation property
    public Provider Provider { get; private set; } = null!;

    private TechSupportAccount() { }

    public TechSupportAccount(string providerId, string identityUserId, string fullName)
    {
        Id = Guid.NewGuid();
        ProviderId = providerId;
        IdentityUserId = identityUserId;
        FullName = fullName;
        IsActive = true;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
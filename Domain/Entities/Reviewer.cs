namespace AcademicGateway.Domain.Entities;

public class Reviewer
{
    public Guid Id { get; private set; }
    public string IdentityUserId { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;

    // EF Core Required Constructor
    private Reviewer() { }

    public Reviewer(Guid id, string identityUserId, string fullName)
    {
        Id = id;
        IdentityUserId = identityUserId;
        FullName = fullName;
    }
}
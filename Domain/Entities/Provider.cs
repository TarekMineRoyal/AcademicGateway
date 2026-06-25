namespace AcademicGateway.Domain.Entities;

public class Provider
{
    // Acts as PK and FK to the Identity User
    public string UserId { get; set; } = string.Empty;

    public string OrganizationName { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string? WebsiteUrl { get; set; }

    // Defaulting to false until verified by a Reviewer
    public bool IsVerified { get; set; } = false;
}
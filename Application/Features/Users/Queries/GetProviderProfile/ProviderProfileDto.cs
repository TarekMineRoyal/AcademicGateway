namespace AcademicGateway.Application.Features.Users.Queries.GetProviderProfile;

public record ProviderProfileDto
{
    public Guid UserId { get; init; }
    public string OrganizationName { get; init; } = string.Empty;
    public string Industry { get; init; } = string.Empty;
    public string? WebsiteUrl { get; init; }
    public bool IsVerified { get; init; }
}
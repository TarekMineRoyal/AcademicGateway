using Microsoft.AspNetCore.Identity;

namespace AcademicGateway.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    // You can add custom authentication-related properties here later if needed
    // (e.g., RefreshToken, LastLoginDate).
    // The profile data (Major, OrganizationName) safely lives in the Domain layer!
}
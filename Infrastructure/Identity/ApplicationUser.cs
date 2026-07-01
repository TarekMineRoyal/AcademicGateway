using Microsoft.AspNetCore.Identity;
using System;

namespace AcademicGateway.Infrastructure.Identity;

/// <summary>
/// Represents the centralized security identity account credential model within the infrastructure tier.
/// Extends the default ASP.NET Core Identity membership system using <see cref="Guid"/> keys.
/// All domain-specific profiles and business rules live strictly encapsulated inside the Domain project layer.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    // Implementation placeholder: Custom infrastructure authentication attributes 
    // (such as RefreshToken strings or LastLoginDate timestamps) can be natively attached here if required later.
}
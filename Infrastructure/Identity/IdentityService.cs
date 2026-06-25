using AcademicGateway.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AcademicGateway.Infrastructure.Identity;

public class IdentityService(UserManager<ApplicationUser> userManager) : IIdentityService
{
    public async Task<(bool Succeeded, string UserId, IEnumerable<string> Errors)> CreateUserAsync(string userName, string email, string password)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
        };

        // This creates the user and hashes the password automatically
        var result = await userManager.CreateAsync(user, password);

        return (
            result.Succeeded,
            user.Id,
            result.Errors.Select(e => e.Description)
        );
    }
}
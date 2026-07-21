using AcademicGateway.Domain.Common.Constants;
using AcademicGateway.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace AcademicGateway.Infrastructure.Persistence.Context.Seeders;

/// <summary>
/// Seeder responsible for registering infrastructure roles and provisioning initial administrative accounts.
/// </summary>
public static class IdentitySeeder
{
    /// <summary>
    /// Seeds system identity roles (including Admin) and provisions the default system administrator.
    /// </summary>
    public static async Task SeedAsync(
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        // 1. Seed System Roles utilizing Explicit Guid configuration types
        string[] roles =
        {
            Roles.Admin,
            Roles.Reviewer,
            Roles.Student,
            Roles.Provider,
            Roles.Professor,
            Roles.TechSupport
        };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }

        // 2. Seed Default System Admin Identity Account from Configuration / Secrets
        var defaultAdminEmail = configuration["DefaultAdmin:Email"] ?? "admin@academicgateway.com";
        var defaultAdminPassword = configuration["DefaultAdmin:Password"] ?? "AdminPassword123!";

        var defaultAdminUser = await userManager.FindByEmailAsync(defaultAdminEmail);

        if (defaultAdminUser == null)
        {
            defaultAdminUser = new ApplicationUser
            {
                UserName = defaultAdminEmail,
                Email = defaultAdminEmail,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(defaultAdminUser, defaultAdminPassword);
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(defaultAdminUser, Roles.Admin);
            }
        }
    }
}
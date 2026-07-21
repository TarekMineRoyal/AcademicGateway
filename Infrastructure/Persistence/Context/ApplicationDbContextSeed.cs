using AcademicGateway.Infrastructure.Identity;
using AcademicGateway.Infrastructure.Persistence.Context.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace AcademicGateway.Infrastructure.Persistence.Context;

/// <summary>
/// Central orchestrator executing modular seeders for system roles, default accounts,
/// curriculum lookups, skills, domain profiles, and project workspaces.
/// </summary>
public static class ApplicationDbContextSeed
{
    /// <summary>
    /// Orchestrates the execution of all module-specific seeders in a strict dependency sequence.
    /// </summary>
    public static async Task SeedDefaultUserAndDataAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ApplicationDbContext context,
        IConfiguration configuration)
    {
        // 1. Seed System Identity Roles & Default Admin Account
        await IdentitySeeder.SeedAsync(roleManager, userManager, configuration);

        // 2. Seed Base Curriculum Lookups (Majors & Specialties)
        await CurriculumSeeder.SeedAsync(context);

        // 3. Seed Base Platform Skills
        await SkillSeeder.SeedAsync(context);

        // 4. Seed Domain Sample Accounts & Profiles (Reviewer, Provider, Professor, Student, Tech Support)
        await DomainEntitySeeder.SeedAsync(userManager, context);

        // 5. Seed Project Templates & Live Workspace Instance
        await ProjectTemplateSeeder.SeedAsync(context, userManager);
    }
}
using AcademicGateway.Infrastructure.Identity;
using Domain.Skills;
using Domain.SystemStaff;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AcademicGateway.Infrastructure.Persistence;

/// <summary>
/// Core structural data seeder executing framework role allocations, system-level lookup values, 
/// and administrative testing profiles inside development environments.
/// </summary>
public static class ApplicationDbContextSeed
{
    /// <summary>
    /// Evaluates and seeds essential infrastructure credentials, lookup dictionary matrix tables, and testing profiles.
    /// </summary>
    public static async Task SeedDefaultUserAndDataAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager, // Realigned signature matching Guid identity configurations
        ApplicationDbContext context)
    {
        // 1. Seed System Roles utilizing Explicit Guid configuration types
        string[] roles = ["Administrator", "Reviewer", "Student", "Provider", "Professor"];

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }

        // 2. Seed Default Reviewer Identity Account
        var defaultReviewerEmail = "reviewer@academicgateway.com";
        var defaultReviewerUser = await userManager.FindByEmailAsync(defaultReviewerEmail);

        if (defaultReviewerUser == null)
        {
            defaultReviewerUser = new ApplicationUser
            {
                UserName = defaultReviewerEmail,
                Email = defaultReviewerEmail,
                EmailConfirmed = true
            };

            // Provision user with standard complexity password parameters
            await userManager.CreateAsync(defaultReviewerUser, "GatewayReviewer123!");
            await userManager.AddToRoleAsync(defaultReviewerUser, "Reviewer");
        }

        // 3. Seed Default Reviewer Domain Profile Entity
        // In the refactored 1:1 domain design, the Profile Id maps directly to the user authentication Id
        if (!await context.Reviewers.AnyAsync(r => r.Id == defaultReviewerUser.Id))
        {
            var reviewerProfile = new Reviewer(
                id: defaultReviewerUser.Id,
                fullName: "Internal Platform Reviewer"
            );

            await context.Reviewers.AddAsync(reviewerProfile);
            await context.SaveChangesAsync(default);
        }

        // 4. Seed Base Lookup Skills via its behavior-driven constructor rules
        if (!await context.Skills.AnyAsync())
        {
            var defaultSkills = new List<Skill>
            {
                new("C# .NET Backend Development"),
                new("React TypeScript Frontend"),
                new("Python Machine Learning"),
                new("PostgreSQL Database Design"),
                new("Docker Containerization")
            };

            await context.Skills.AddRangeAsync(defaultSkills);
            await context.SaveChangesAsync(default);
        }
    }
}
using AcademicGateway.Infrastructure.Identity;
using Domain.Lookups;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AcademicGateway.Infrastructure.Persistence;

public static class ApplicationDbContextSeed
{
    public static async Task SeedDefaultUserAndDataAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context)
    {
        // 1. Seed System Roles
        string[] roles = ["Administrator", "Reviewer", "Student", "Provider", "Professor"];

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
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

            // Using a standard strong password for development environment testing
            await userManager.CreateAsync(defaultReviewerUser, "GatewayReviewer123!");
            await userManager.AddToRoleAsync(defaultReviewerUser, "Reviewer");
        }

        // 3. Seed Default Reviewer Domain Profile Entity
        if (!await context.Reviewers.AnyAsync(r => r.IdentityUserId == defaultReviewerUser.Id))
        {
            var reviewerProfile = new Reviewer(
                id: Guid.NewGuid(),
                identityUserId: defaultReviewerUser.Id,
                fullName: "Internal Platform Reviewer"
            );

            await context.Reviewers.AddAsync(reviewerProfile);
            await context.SaveChangesAsync(default);
        }

        // 4. Seed Base Lookup Skills (Essential for Content Engine/Templates testing)
        if (!await context.Skills.AnyAsync())
        {
            var defaultSkills = new List<Skill>
            {
                new Skill { Id = Guid.NewGuid(), Name = "C# .NET Backend Development" },
                new Skill { Id = Guid.NewGuid(), Name = "React TypeScript Frontend" },
                new Skill { Id = Guid.NewGuid(), Name = "Python Machine Learning" },
                new Skill { Id = Guid.NewGuid(), Name = "PostgreSQL Database Design" },
                new Skill { Id = Guid.NewGuid(), Name = "Docker Containerization" }
            };

            await context.Skills.AddRangeAsync(defaultSkills);
            await context.SaveChangesAsync(default);
        }
    }
}
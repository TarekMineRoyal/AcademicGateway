using AcademicGateway.Infrastructure.Identity;
using AcademicGateway.Infrastructure.Persistence.Context;
using AcademicGateway.Domain.Curriculum;
using AcademicGateway.Domain.Skills;
using AcademicGateway.Domain.Reviewers;
using AcademicGateway.Domain.Providers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AcademicGateway.Infrastructure.Persistence.Context;

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
        RoleManager<IdentityRole<Guid>> roleManager,
        ApplicationDbContext context)
    {
        // 1. Seed System Roles utilizing Explicit Guid configuration types
        string[] roles = { "Administrator", "Reviewer", "Student", "Provider", "Professor" };

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

            await userManager.CreateAsync(defaultReviewerUser, "GatewayReviewer123!");
            await userManager.AddToRoleAsync(defaultReviewerUser, "Reviewer");
        }

        // 3. Seed Default Reviewer Domain Profile Entity
        if (!await context.Reviewers.AnyAsync(r => r.Id == defaultReviewerUser.Id))
        {
            var reviewerProfile = new Reviewer(
                id: defaultReviewerUser.Id,
                fullName: "Internal Platform Reviewer"
            );

            await context.Reviewers.AddAsync(reviewerProfile);
            await context.SaveChangesAsync();
        }

        // 4. Seed Default Provider Identity Account (For Onboarding Workflow Validation)
        var defaultProviderEmail = "partner@acmesolutions.internal";
        var defaultProviderUser = await userManager.FindByEmailAsync(defaultProviderEmail);

        if (defaultProviderUser == null)
        {
            defaultProviderUser = new ApplicationUser
            {
                UserName = defaultProviderEmail,
                Email = defaultProviderEmail,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(defaultProviderUser, "CorporatePartner123!");
            await userManager.AddToRoleAsync(defaultProviderUser, "Provider");
        }

        // 5. Seed Default Unverified Provider Domain Profile Entity
        if (!await context.Providers.AnyAsync(p => p.Id == defaultProviderUser.Id))
        {
            var providerProfile = new Provider(
                id: defaultProviderUser.Id,
                companyName: "Acme Corporate Innovations"
            );

            providerProfile.UpdateProfileDetails(
                description: "Global enterprise specializing in cloud computing, infrastructure architecture, and technical software solutions.",
                websiteUrl: "https://acme-innovations.internal"
            );

            await context.Providers.AddAsync(providerProfile);
            await context.SaveChangesAsync();
        }

        // 6. Seed a Pending Onboarding Application to populate the Reviewer Dashboards instantly
        if (!await context.ProviderApplications.AnyAsync(a => a.ProviderId == defaultProviderUser.Id))
        {
            var pendingApplication = new ProviderApplication(
                providerId: defaultProviderUser.Id,
                companyDetails: "Acme is seeking platform verification to sponsor high-scale distributed systems design and microservice architecture projects for final-year computer science tracks.",
                verificationDocumentsUrl: "https://storage.academicgateway.internal/onboarding/acme-credentials.pdf",
                createdAt: DateTime.UtcNow.AddDays(-2) // Submitted 2 days ago to simulate realistic operational backlog
            );

            // Transition the application out of Draft and straight into the Reviewer operational pool
            pendingApplication.SubmitForReview();

            await context.ProviderApplications.AddAsync(pendingApplication);
            await context.SaveChangesAsync();
        }

        // 7. Seed Base Lookup Skills via its behavior-driven constructor rules
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
            await context.SaveChangesAsync();
        }

        // 8. Seed Majors and Specialties with their respective domain-driven design rules
        if (!await context.Majors.AnyAsync())
        {
            var computerScience = new Major("Computer Science");
            computerScience.AddSpecialty("Software Engineering");
            computerScience.AddSpecialty("Artificial Intelligence");
            computerScience.AddSpecialty("Cybersecurity");

            var engineering = new Major("Electrical Engineering");
            engineering.AddSpecialty("Embedded Systems");
            engineering.AddSpecialty("Power Systems");

            await context.Majors.AddRangeAsync(computerScience, engineering);
            await context.SaveChangesAsync();
        }
    }
}
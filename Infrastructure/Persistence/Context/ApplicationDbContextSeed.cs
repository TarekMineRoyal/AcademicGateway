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
using AcademicGateway.Domain.Students;
using AcademicGateway.Domain.ProjectTemplates;
using AcademicGateway.Domain.Professors;
using AcademicGateway.Domain.ProjectInstances.Services;

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

        // 9. Seed Default Professor Identity Account
        var defaultProfessorEmail = "professor@academicgateway.com";
        var defaultProfessorUser = await userManager.FindByEmailAsync(defaultProfessorEmail);

        if (defaultProfessorUser == null)
        {
            defaultProfessorUser = new ApplicationUser
            {
                UserName = defaultProfessorEmail,
                Email = defaultProfessorEmail,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(defaultProfessorUser, "GatewayProfessor123!");
            await userManager.AddToRoleAsync(defaultProfessorUser, "Professor");
        }

        // 10. Seed Default Professor Domain Profile Entity
        if (!await context.Professors.AnyAsync(p => p.Id == defaultProfessorUser.Id))
        {
            var professorProfile = new Professor(
                id: defaultProfessorUser.Id,
                fullName: "Dr. Alan Turing",
                department: "Computer Science",
                rank: "Full Professor",
                maxSupervisionCapacity: 5
            );

            await context.Professors.AddAsync(professorProfile);
            await context.SaveChangesAsync();
        }

        // 11. Seed Default Student Identity Account
        var defaultStudentEmail = "student@academicgateway.com";
        var defaultStudentUser = await userManager.FindByEmailAsync(defaultStudentEmail);

        if (defaultStudentUser == null)
        {
            defaultStudentUser = new ApplicationUser
            {
                UserName = defaultStudentEmail,
                Email = defaultStudentEmail,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(defaultStudentUser, "GatewayStudent123!");
            await userManager.AddToRoleAsync(defaultStudentUser, "Student");
        }

        // 12. Seed Default Student Domain Profile Entity
        if (!await context.Students.AnyAsync(s => s.Id == defaultStudentUser.Id))
        {
            var studentProfile = new Student(
                id: defaultStudentUser.Id,
                fullName: "Jane Doe",
                graduationYear: 2027
            );

            // A. Query a seeded Major along with its Specialties using eager loading (.Include)
            var computerScienceMajor = await context.Majors
                .Include(m => m.Specialties)
                .FirstOrDefaultAsync(m => m.Name == "Computer Science");

            if (computerScienceMajor != null)
            {
                // Assign the Major
                studentProfile.AddMajor(computerScienceMajor.Id);

                // Extract and assign a Specialty under that Major (e.g., Software Engineering)
                var softwareEngineeringSpecialty = computerScienceMajor.Specialties
                    .FirstOrDefault(s => s.Name == "Software Engineering");

                if (softwareEngineeringSpecialty != null)
                {
                    studentProfile.AddSpecialty(softwareEngineeringSpecialty.Id);
                }
            }

            // B. Query a couple of seeded baseline Skills to assign to the student
            var dotNetSkill = await context.Skills
                .FirstOrDefaultAsync(s => s.Name == "C# .NET Backend Development");

            var dockerSkill = await context.Skills
                .FirstOrDefaultAsync(s => s.Name == "Docker Containerization");

            if (dotNetSkill != null)
            {
                studentProfile.AddSkill(dotNetSkill.Id);
            }

            if (dockerSkill != null)
            {
                studentProfile.AddSkill(dockerSkill.Id);
            }

            // C. Save the fully populated Student aggregate root profile context
            await context.Students.AddAsync(studentProfile);
            await context.SaveChangesAsync();
        }

        // 13. Seed 2 Baseline Project Templates & 1 Live Running Instance Workspace Track
        if (!await context.ProjectTemplates.AnyAsync())
        {
            var seededSkills = await context.Skills.ToListAsync();

            // --- Template 1: Cloud Migration ---
            var cloudTemplate = new ProjectTemplate(
                title: "Distributed E-Commerce Cloud Infrastructure Migration",
                description: "Design and implement a fully automated microservices deployment pipeline using Docker and PostgreSQL backend clusters.",
                providerId: defaultProviderUser.Id,
                createdAt: DateTime.UtcNow.AddDays(-5)
            );

            if (seededSkills.Any())
            {
                cloudTemplate.AddSkill(seededSkills.First(s => s.Name.Contains("Docker")).Id);
                cloudTemplate.AddSkill(seededSkills.First(s => s.Name.Contains("PostgreSQL")).Id);
            }

            cloudTemplate.AddMilestone(
                title: "Architecture & Schema Topology Draft",
                description: "Submit a complete entity relational chart along with cloud infrastructure networking layouts.",
                expectedEffortInHours: 15.5m,
                deliverableType: AcademicGateway.Domain.Common.Enums.DeliverableType.File
            );

            cloudTemplate.SubmitForReview();
            cloudTemplate.Approve();

            await context.ProjectTemplates.AddAsync(cloudTemplate);

            // --- Template 2: Predictive Analytics (NEW) ---
            var analyticsTemplate = new ProjectTemplate(
                title: "Enterprise Predictive Analytics Dashboard Engine",
                description: "Develop an end-to-end predictive analytics data pipeline using Python ML modeling, served via an interactive React TypeScript tracking interface.",
                providerId: defaultProviderUser.Id,
                createdAt: DateTime.UtcNow.AddDays(-3)
            );

            if (seededSkills.Any())
            {
                analyticsTemplate.AddSkill(seededSkills.First(s => s.Name.Contains("Python")).Id);
                analyticsTemplate.AddSkill(seededSkills.First(s => s.Name.Contains("React")).Id);
            }

            analyticsTemplate.AddMilestone(
                title: "Model Training Evaluation & API Specification",
                description: "Train the core predictive model variant and publish formal OpenAPI endpoint execution schemas.",
                expectedEffortInHours: 22.0m,
                deliverableType: AcademicGateway.Domain.Common.Enums.DeliverableType.File
            );

            analyticsTemplate.SubmitForReview();
            analyticsTemplate.Approve();

            await context.ProjectTemplates.AddAsync(analyticsTemplate);

            // Commit both templates to the database
            await context.SaveChangesAsync();

            // 14. Instantiate Live Project Workspace utilizing Domain Prototype and Factory Services
            if (!await context.ProjectInstances.AnyAsync())
            {
                var milestoneFactory = new LocalMilestoneFactory();
                var executionClockSnapshot = DateTime.UtcNow;

                // Manufacture clean running channel context with a tracking supervisor invitation initialized
                var projectInstance = cloudTemplate.Instantiate(
                    studentId: defaultStudentUser.Id,
                    createdAt: executionClockSnapshot,
                    milestoneFactory: milestoneFactory,
                    initialRequestedProfessorId: defaultProfessorUser.Id
                );

                // Isolate and accept the generated matchmaking invitation to safely step State machine indexes into Active mode
                var pendingInvitation = projectInstance.SupervisionRequests.First();
                projectInstance.ReviewSupervisionRequest(
                    requestId: pendingInvitation.Id,
                    accept: true,
                    rejectionReason: null,
                    reviewedAt: executionClockSnapshot
                );

                // Synchronize and update current active supervision loads on the Professor Aggregate Root
                var professorToUpdate = await context.Professors.FindAsync(defaultProfessorUser.Id);
                professorToUpdate?.IncrementActiveProjects();

                // Save both mutated aggregates seamlessly inside the unit-of-work pipeline transaction
                await context.ProjectInstances.AddAsync(projectInstance);
                await context.SaveChangesAsync();
            }
        }
    }
}
using AcademicGateway.Domain.Common.Constants;
using AcademicGateway.Domain.Professors;
using AcademicGateway.Domain.Providers;
using AcademicGateway.Domain.Reviewers;
using AcademicGateway.Domain.Students;
using AcademicGateway.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AcademicGateway.Infrastructure.Persistence.Context.Seeders;

/// <summary>
/// Seeder responsible for populating sample operational profiles and testing user accounts
/// (Reviewers, Providers, Professors, Students, and Technical Support).
/// </summary>
public static class DomainEntitySeeder
{
    /// <summary>
    /// Evaluates and seeds baseline domain entities along with their associated identity accounts.
    /// </summary>
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        // =========================================================================
        // 1. Seed Default Reviewer [source: 2]
        // =========================================================================
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
            await userManager.AddToRoleAsync(defaultReviewerUser, Roles.Reviewer);
        }

        if (!await context.Reviewers.AnyAsync(r => r.Id == defaultReviewerUser.Id))
        {
            var reviewerProfile = new Reviewer(
                id: defaultReviewerUser.Id,
                fullName: "Internal Platform Reviewer"
            );

            await context.Reviewers.AddAsync(reviewerProfile);
            await context.SaveChangesAsync();
        }

        // =========================================================================
        // 2. Seed Default Provider & Pending Application [source: 2]
        // =========================================================================
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
            await userManager.AddToRoleAsync(defaultProviderUser, Roles.Provider);
        }

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

        if (!await context.ProviderApplications.AnyAsync(a => a.ProviderId == defaultProviderUser.Id))
        {
            var pendingApplication = new ProviderApplication(
                providerId: defaultProviderUser.Id,
                companyDetails: "Acme is seeking platform verification to sponsor high-scale distributed systems design and microservice architecture projects for final-year computer science tracks.",
                verificationDocumentsUrl: "https://storage.academicgateway.internal/onboarding/acme-credentials.pdf",
                createdAt: DateTime.UtcNow.AddDays(-2)
            );

            pendingApplication.SubmitForReview();

            await context.ProviderApplications.AddAsync(pendingApplication);
            await context.SaveChangesAsync();
        }

        // =========================================================================
        // 3. Seed Default Professor [source: 2]
        // =========================================================================
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
            await userManager.AddToRoleAsync(defaultProfessorUser, Roles.Professor);
        }

        if (!await context.Professors.AnyAsync(p => p.Id == defaultProfessorUser.Id))
        {
            var professorProfile = new Professor(
                id: defaultProfessorUser.Id,
                fullName: "Dr. Alan Turing",
                department: "Computer Science",
                rank: "Full Professor",
                maxSupervisionCapacity: 5
            );

            professorProfile.UpdateAboutMe("Passionate about distributed systems, theoretical computer science, and advising innovative capstone projects.");

            await context.Professors.AddAsync(professorProfile);
            await context.SaveChangesAsync();
        }

        // =========================================================================
        // 4. Seed Default Student [source: 2]
        // =========================================================================
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
            await userManager.AddToRoleAsync(defaultStudentUser, Roles.Student);
        }

        if (!await context.Students.AnyAsync(s => s.Id == defaultStudentUser.Id))
        {
            var studentProfile = new Student(
                id: defaultStudentUser.Id,
                fullName: "Jane Doe",
                graduationYear: 2027
            );

            studentProfile.UpdateAboutMe("Final-year Computer Science student specializing in Software Engineering, interested in backend C# development and cloud architecture.");

            var computerScienceMajor = await context.Majors
                .Include(m => m.Specialties)
                .FirstOrDefaultAsync(m => m.Name == "Computer Science");

            if (computerScienceMajor != null)
            {
                studentProfile.AddMajor(computerScienceMajor.Id);

                var softwareEngineeringSpecialty = computerScienceMajor.Specialties
                    .FirstOrDefault(s => s.Name == "Software Engineering");

                if (softwareEngineeringSpecialty != null)
                {
                    studentProfile.AddSpecialty(softwareEngineeringSpecialty.Id);
                }
            }

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

            await context.Students.AddAsync(studentProfile);
            await context.SaveChangesAsync();
        }

        // =========================================================================
        // 5. Seed Verified Provider & Technical Support Account [source: 2]
        // =========================================================================
        var verifiedProviderEmail = "verified-partner@cloudsystems.internal";
        var verifiedProviderUser = await userManager.FindByEmailAsync(verifiedProviderEmail);

        if (verifiedProviderUser == null)
        {
            verifiedProviderUser = new ApplicationUser
            {
                UserName = verifiedProviderEmail,
                Email = verifiedProviderEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(verifiedProviderUser, "VerifiedPartner123!");
            await userManager.AddToRoleAsync(verifiedProviderUser, Roles.Provider);
        }

        if (!await context.Providers.AnyAsync(p => p.Id == verifiedProviderUser.Id))
        {
            var verifiedProviderProfile = new Provider(
                id: verifiedProviderUser.Id,
                companyName: "Cloud Systems Architectures"
            );
            verifiedProviderProfile.UpdateProfileDetails(
                description: "Verified infrastructure firm specializing in hyper-scale cluster engineering guidance.",
                websiteUrl: "https://cloudsystems.internal"
            );

            verifiedProviderProfile.VerifyProfile();

            await context.Providers.AddAsync(verifiedProviderProfile);
            await context.SaveChangesAsync();
        }

        var techSupportEmail = "mentor.alan@cloudsystems.internal";
        var techSupportUser = await userManager.FindByEmailAsync(techSupportEmail);

        if (techSupportUser == null)
        {
            techSupportUser = new ApplicationUser
            {
                UserName = techSupportEmail,
                Email = techSupportEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(techSupportUser, "SecureTechSupport123!");
            await userManager.AddToRoleAsync(techSupportUser, Roles.TechSupport);
        }

        if (!await context.TechSupportAccounts.AnyAsync(ts => ts.Id == techSupportUser.Id))
        {
            var techSupportAccount = new TechSupportAccount(
                id: techSupportUser.Id,
                providerId: verifiedProviderUser.Id,
                staffNumber: "CS-G9-011",
                supportTier: "Tier 3 Systems Architect"
            );

            await context.TechSupportAccounts.AddAsync(techSupportAccount);
            await context.SaveChangesAsync();
        }
    }
}
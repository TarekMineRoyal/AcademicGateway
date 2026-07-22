using AcademicGateway.Domain.Common.Constants;
using AcademicGateway.Domain.Providers;
using AcademicGateway.Infrastructure.Identity;
using Infrastructure.Persistence.Context.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AcademicGateway.Infrastructure.Persistence.Context.Seeders.DomainEntitySeeders;

/// <summary>
/// Seeder responsible for populating industry provider company user accounts,
/// profiles, and onboarding applications across all lifecycle states 
/// (Pending, Approved/Verified, Rejected, and Resubmitted).
/// </summary>
public static class ProviderSeeder
{
    /// <summary>
    /// Evaluates and seeds baseline corporate provider personas and application queues.
    /// </summary>
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        var reviewerId = SeedConstants.Reviewers.IndustryTrackReviewerId;

        // =========================================================================
        // 1. Acme Corporate Innovations (Pending Review Application)
        // =========================================================================
        var acmeUser = await SeedProviderIdentityAsync(
            userManager,
            SeedConstants.Providers.AcmeCorpId,
            SeedConstants.Providers.AcmeCorpEmail);

        if (!await context.Providers.AnyAsync(p => p.Id == acmeUser.Id))
        {
            var acmeProvider = new Provider(acmeUser.Id, "Acme Corporate Innovations");
            acmeProvider.UpdateProfileDetails(
                description: "Global enterprise specializing in cloud computing, infrastructure architecture, and technical software solutions.",
                websiteUrl: "https://acme-innovations.internal"
            );

            await context.Providers.AddAsync(acmeProvider);
        }

        if (!await context.ProviderApplications.AnyAsync(a => a.ProviderId == acmeUser.Id))
        {
            var acmeApp = new ProviderApplication(
                providerId: acmeUser.Id,
                companyDetails: "Acme is seeking platform verification to sponsor high-scale distributed systems design and microservice architecture projects for final-year computer science tracks.",
                verificationDocumentsUrl: "https://storage.academicgateway.internal/onboarding/acme-credentials.pdf",
                createdAt: DateTime.UtcNow.AddDays(-2)
            );

            acmeApp.SubmitForReview();
            await context.ProviderApplications.AddAsync(acmeApp);
        }

        // =========================================================================
        // 2. Cloud Systems Architectures (Verified & Approved Application)
        // =========================================================================
        var cloudUser = await SeedProviderIdentityAsync(
            userManager,
            SeedConstants.Providers.CloudSystemsId,
            SeedConstants.Providers.CloudSystemsEmail);

        if (!await context.Providers.AnyAsync(p => p.Id == cloudUser.Id))
        {
            var cloudProvider = new Provider(cloudUser.Id, "Cloud Systems Architectures");
            cloudProvider.UpdateProfileDetails(
                description: "Verified infrastructure firm specializing in hyper-scale cluster engineering guidance, DevOps mentorship, and SRE best practices.",
                websiteUrl: "https://cloudsystems.internal"
            );

            cloudProvider.VerifyProfile(); // Mark profile as verified
            await context.Providers.AddAsync(cloudProvider);
        }

        if (!await context.ProviderApplications.AnyAsync(a => a.ProviderId == cloudUser.Id))
        {
            var cloudApp = new ProviderApplication(
                providerId: cloudUser.Id,
                companyDetails: "Cloud Systems Architectures application for verified industry partnership to co-advise cloud infrastructure capstone projects.",
                verificationDocumentsUrl: "https://storage.academicgateway.internal/onboarding/cloudsystems-verification.pdf",
                createdAt: DateTime.UtcNow.AddDays(-30)
            );

            cloudApp.SubmitForReview();

            // Corrected: Passing required reviewerId and approval date arguments
            cloudApp.Approve(
                reviewerId: reviewerId,
                approvedAt: DateTime.UtcNow.AddDays(-28)
            );

            await context.ProviderApplications.AddAsync(cloudApp);
        }

        // =========================================================================
        // 3. CyberShield Dynamics (Rejected Application Edge Case)
        // =========================================================================
        var shieldUser = await SeedProviderIdentityAsync(
            userManager,
            SeedConstants.Providers.CyberShieldId,
            SeedConstants.Providers.CyberShieldEmail);

        if (!await context.Providers.AnyAsync(p => p.Id == shieldUser.Id))
        {
            var shieldProvider = new Provider(shieldUser.Id, "CyberShield Dynamics");
            shieldProvider.UpdateProfileDetails(
                description: "Cybersecurity agency focused on threat intelligence and offensive network penetration testing.",
                websiteUrl: "https://cybershield.internal"
            );

            await context.Providers.AddAsync(shieldProvider);
        }

        if (!await context.ProviderApplications.AnyAsync(a => a.ProviderId == shieldUser.Id))
        {
            var shieldApp = new ProviderApplication(
                providerId: shieldUser.Id,
                companyDetails: "Seeking partner status for cybersecurity laboratory sponsorship.",
                verificationDocumentsUrl: "https://storage.academicgateway.internal/onboarding/cybershield-tax-id.pdf",
                createdAt: DateTime.UtcNow.AddDays(-14)
            );

            shieldApp.SubmitForReview();

            // Corrected: Passing required reviewerId, rejection reason, and timestamp arguments
            shieldApp.Reject(
                reviewerId: reviewerId,
                reason: "Invalid corporate tax documentation provided.",
                rejectedAt: DateTime.UtcNow.AddDays(-12)
            );

            await context.ProviderApplications.AddAsync(shieldApp);
        }

        // =========================================================================
        // 4. Apex Game Studios (Resubmitted / Pending Secondary Review Application)
        // =========================================================================
        var apexUser = await SeedProviderIdentityAsync(
            userManager,
            SeedConstants.Providers.ApexGamesId,
            SeedConstants.Providers.ApexGamesEmail);

        if (!await context.Providers.AnyAsync(p => p.Id == apexUser.Id))
        {
            var apexProvider = new Provider(apexUser.Id, "Apex Game Studios");
            apexProvider.UpdateProfileDetails(
                description: "Interactive gaming studio specializing in 3D graphics rendering engines, physics simulation, and real-time multiplayer networking.",
                websiteUrl: "https://apexgames.internal"
            );

            await context.Providers.AddAsync(apexProvider);
        }

        if (!await context.ProviderApplications.AnyAsync(a => a.ProviderId == apexUser.Id))
        {
            var apexApp = new ProviderApplication(
                providerId: apexUser.Id,
                companyDetails: "Apex Game Studios initial application for interactive graphics capstone mentorship.",
                verificationDocumentsUrl: "https://storage.academicgateway.internal/onboarding/apexgames-v1.pdf",
                createdAt: DateTime.UtcNow.AddDays(-10)
            );

            apexApp.SubmitForReview();

            // Corrected: Passing required reviewerId, rejection reason, and timestamp arguments
            apexApp.Reject(
                reviewerId: reviewerId,
                reason: "Initial submission lacked official corporate registration certificate.",
                rejectedAt: DateTime.UtcNow.AddDays(-8)
            );

            // Resubmit shifts status back from Rejected -> PendingReview
            apexApp.Resubmit(
                newCompanyDetails: "Apex Game Studios resubmitted application with valid registration and tax identification certificate.",
                newVerificationDocumentsUrl: "https://storage.academicgateway.internal/onboarding/apexgames-v2-verified.pdf"
            );

            await context.ProviderApplications.AddAsync(apexApp);
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Helper method to guarantee Identity user account creation with Provider role.
    /// </summary>
    private static async Task<ApplicationUser> SeedProviderIdentityAsync(
        UserManager<ApplicationUser> userManager,
        Guid userId,
        string email)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new ApplicationUser
            {
                Id = userId,
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, SeedConstants.DefaultPassword);
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(user, Roles.Provider);
            }
        }

        return user;
    }
}
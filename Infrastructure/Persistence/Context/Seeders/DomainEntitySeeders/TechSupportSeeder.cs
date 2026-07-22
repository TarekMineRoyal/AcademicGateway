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
/// Seeder responsible for populating technical support mentor user accounts and profiles.
/// Binds tech support accounts directly to existing corporate providers via ProviderId foreign keys.
/// </summary>
public static class TechSupportSeeder
{
    /// <summary>
    /// Evaluates and seeds baseline technical support accounts linked to corporate providers.
    /// </summary>
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        var accountsToSeed = new[]
        {
            // 1. Alan Vance - Tier 3 Systems Architect (Attached to Verified Provider: Cloud Systems)
            (
                Id: SeedConstants.TechSupport.AlanVanceId,
                Email: SeedConstants.TechSupport.AlanVanceEmail,
                ProviderId: SeedConstants.Providers.CloudSystemsId,
                StaffNumber: "CS-G9-011",
                SupportTier: "Tier 3 Systems Architect"
            ),

            // 2. Sarah Connor - Tier 1 DevOps Mentor (Attached to Verified Provider: Cloud Systems)
            (
                Id: SeedConstants.TechSupport.SarahConnorId,
                Email: SeedConstants.TechSupport.SarahConnorEmail,
                ProviderId: SeedConstants.Providers.CloudSystemsId,
                StaffNumber: "CS-G9-012",
                SupportTier: "Tier 1 DevOps Mentor"
            ),

            // 3. Devon Miles - Security Operations Analyst (Attached to CyberShield Dynamics)
            (
                Id: SeedConstants.TechSupport.DevonMilesId,
                Email: SeedConstants.TechSupport.DevonMilesEmail,
                ProviderId: SeedConstants.Providers.CyberShieldId,
                StaffNumber: "CS-G9-013",
                SupportTier: "Security Operations Analyst"
            )
        };

        foreach (var data in accountsToSeed)
        {
            // Foreign Key Safety Guard: Verify target provider exists in database before proceeding
            if (!await context.Providers.AnyAsync(p => p.Id == data.ProviderId))
            {
                continue;
            }

            // 1. Identity Account Provisioning
            var user = await userManager.FindByEmailAsync(data.Email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    Id = data.Id,
                    UserName = data.Email,
                    Email = data.Email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user, SeedConstants.DefaultPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, Roles.TechSupport);
                }
            }

            // 2. Domain TechSupportAccount Provisioning
            if (!await context.TechSupportAccounts.AnyAsync(ts => ts.Id == user.Id))
            {
                var techSupportAccount = new TechSupportAccount(
                    id: user.Id,
                    providerId: data.ProviderId,
                    staffNumber: data.StaffNumber,
                    supportTier: data.SupportTier
                );

                await context.TechSupportAccounts.AddAsync(techSupportAccount);
            }
        }

        await context.SaveChangesAsync();
    }
}
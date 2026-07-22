using AcademicGateway.Domain.Common.Constants;
using AcademicGateway.Domain.Reviewers;
using AcademicGateway.Infrastructure.Identity;
using Infrastructure.Persistence.Context.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AcademicGateway.Infrastructure.Persistence.Context.Seeders.DomainEntitySeeders;

/// <summary>
/// Seeder responsible for populating internal and external reviewer identity user accounts and domain profiles.
/// </summary>
public static class ReviewerSeeder
{
    /// <summary>
    /// Evaluates and seeds baseline reviewer personas across academic and industry tracks.
    /// </summary>
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        var reviewersToSeed = new[]
        {
            (
                Id: SeedConstants.Reviewers.LeadReviewerId,
                Email: SeedConstants.Reviewers.LeadReviewerEmail,
                FullName: "Lead Academic Reviewer"
            ),
            (
                Id: SeedConstants.Reviewers.SeniorCurriculumReviewerId,
                Email: SeedConstants.Reviewers.SeniorCurriculumReviewerEmail,
                FullName: "Senior Curriculum Reviewer"
            ),
            (
                Id: SeedConstants.Reviewers.IndustryTrackReviewerId,
                Email: SeedConstants.Reviewers.IndustryTrackReviewerEmail,
                FullName: "Industry Track Reviewer"
            )
        };

        foreach (var reviewerData in reviewersToSeed)
        {
            var user = await userManager.FindByEmailAsync(reviewerData.Email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    Id = reviewerData.Id,
                    UserName = reviewerData.Email,
                    Email = reviewerData.Email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user, SeedConstants.DefaultPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, Roles.Reviewer);
                }
            }

            if (!await context.Reviewers.AnyAsync(r => r.Id == user.Id))
            {
                var reviewerProfile = new Reviewer(
                    id: user.Id,
                    fullName: reviewerData.FullName
                );

                await context.Reviewers.AddAsync(reviewerProfile);
            }
        }

        await context.SaveChangesAsync();
    }
}
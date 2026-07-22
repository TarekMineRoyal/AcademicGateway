using AcademicGateway.Infrastructure.Identity;
using AcademicGateway.Infrastructure.Persistence.Context.Seeders.DomainEntitySeeders;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace AcademicGateway.Infrastructure.Persistence.Context.Seeders;

/// <summary>
/// Orchestrator seeder responsible for delegating baseline domain entity 
/// and user profile seeding to specialized modular seeders in strict dependency order.
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
        // 1. Seed Reviewers (Independent domain actors)
        await ReviewerSeeder.SeedAsync(userManager, context);

        // 2. Seed Professors & Research Interests (For AI matchmaking & recommendations)
        await ProfessorSeeder.SeedAsync(userManager, context);

        // 3. Seed Students (Mapped to previously seeded Majors, Specialties, and Skills)
        await StudentSeeder.SeedAsync(userManager, context);

        // 4. Seed Corporate Industry Providers & Applications (Across all lifecycle states)
        await ProviderSeeder.SeedAsync(userManager, context);

        // 5. Seed Technical Support Mentors (Linked to existing Providers via ProviderId FKs)
        await TechSupportSeeder.SeedAsync(userManager, context);
    }
}
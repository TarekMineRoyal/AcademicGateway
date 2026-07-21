using AcademicGateway.Domain.Curriculum;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AcademicGateway.Infrastructure.Persistence.Context.Seeders;

/// <summary>
/// Seeder responsible for populating baseline academic curriculum entities (Majors and Specialties).
/// </summary>
public static class CurriculumSeeder
{
    /// <summary>
    /// Evaluates and seeds default majors along with their corresponding specialties.
    /// </summary>
    public static async Task SeedAsync(ApplicationDbContext context)
    {
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
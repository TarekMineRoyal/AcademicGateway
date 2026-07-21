using AcademicGateway.Domain.Skills;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcademicGateway.Infrastructure.Persistence.Context.Seeders;

/// <summary>
/// Seeder responsible for populating baseline platform skills.
/// </summary>
public static class SkillSeeder
{
    /// <summary>
    /// Evaluates and seeds default skills lookup dictionary values.
    /// </summary>
    public static async Task SeedAsync(ApplicationDbContext context)
    {
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
    }
}
using AcademicGateway.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcademicGateway.Infrastructure.Persistence;

public static class ApplicationDbContextSeed
{
    public static async Task SeedSampleDataAsync(ApplicationDbContext context)
    {
        // 1. Seed Skills
        if (!await context.Skills.AnyAsync())
        {
            context.Skills.AddRange(new List<Skill>
            {
                new Skill { Id = Guid.NewGuid(), Name = "C#" },
                new Skill { Id = Guid.NewGuid(), Name = "Python" },
                new Skill { Id = Guid.NewGuid(), Name = "System Architecture" },
                new Skill { Id = Guid.NewGuid(), Name = "Neural Networks" }
            });

            await context.SaveChangesAsync();
        }

        // 2. Seed Majors and their respective Specialties
        if (!await context.Majors.AnyAsync())
        {
            var csMajor = new Major
            {
                Id = Guid.NewGuid(),
                Name = "Computer Science",
                Specialties = new List<Specialty>
                {
                    new Specialty { Id = Guid.NewGuid(), Name = "Software Engineering" },
                    new Specialty { Id = Guid.NewGuid(), Name = "Artificial Intelligence" }
                }
            };

            var businessMajor = new Major
            {
                Id = Guid.NewGuid(),
                Name = "Business Administration",
                Specialties = new List<Specialty>
                {
                    new Specialty { Id = Guid.NewGuid(), Name = "Finance" },
                    new Specialty { Id = Guid.NewGuid(), Name = "Marketing" }
                }
            };

            context.Majors.AddRange(csMajor, businessMajor);
            await context.SaveChangesAsync();
        }
    }
}
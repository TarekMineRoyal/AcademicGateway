using AcademicGateway.Domain.Common.Constants;
using AcademicGateway.Domain.Professors;
using AcademicGateway.Infrastructure.Identity;
using Infrastructure.Persistence.Context.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcademicGateway.Infrastructure.Persistence.Context.Seeders.DomainEntitySeeders;

/// <summary>
/// Seeder responsible for populating specialized professor user accounts, 
/// academic profiles, bios, and domain research interest tags.
/// Designed specifically to provide rich, distinct domain signals for 
/// AI matchmaking and professor recommendation queries.
/// </summary>
public static class ProfessorSeeder
{
    /// <summary>
    /// Evaluates and seeds baseline professor archetypes across departments.
    /// </summary>
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        var professorsToSeed = new[]
        {
            // 1. Computer Science / Distributed Systems
            (
                Id: SeedConstants.Professors.AlanTuringId,
                Email: SeedConstants.Professors.AlanTuringEmail,
                FullName: "Dr. Alan Turing",
                Department: "Computer Science",
                Rank: "Full Professor",
                MaxCapacity: 5,
                AboutMe: "Passionate about distributed systems, theoretical computer science, microservices architecture, and cloud consensus algorithms.",
                ResearchInterests: new[] { "Distributed Systems", "Cloud Architecture", "Microservices", "Consensus Algorithms", "Kubernetes" }
            ),

            // 2. AI / Deep Learning (Supervision Capacity Reached Edge Case)
            (
                Id: SeedConstants.Professors.GeoffreyHintonId,
                Email: SeedConstants.Professors.GeoffreyHintonEmail,
                FullName: "Dr. Geoffrey Hinton",
                Department: "Artificial Intelligence & Data Science",
                Rank: "Distinguished Professor",
                MaxCapacity: 2, // Low capacity edge case for limit testing
                AboutMe: "Pioneer in artificial neural networks, deep learning architectures, large language models (LLMs), and computer vision perception.",
                ResearchInterests: new[] { "Deep Learning", "Large Language Models", "Computer Vision", "Neural Networks", "PyTorch" }
            ),

            // 3. Cybersecurity & Cryptography
            (
                Id: SeedConstants.Professors.AdaLovelaceId,
                Email: SeedConstants.Professors.AdaLovelaceEmail,
                FullName: "Dr. Ada Lovelace",
                Department: "Cybersecurity & Network Infrastructure",
                Rank: "Associate Professor",
                MaxCapacity: 4,
                AboutMe: "Specializing in offensive security, zero-trust network architecture, elliptic curve cryptography, threat modeling, and container defense.",
                ResearchInterests: new[] { "Offensive Security", "Zero-Trust Architecture", "Cryptography", "Threat Modeling", "Container Security" }
            ),

            // 4. Computer Engineering / IoT & Embedded Systems
            (
                Id: SeedConstants.Professors.ClaudeShannonId,
                Email: SeedConstants.Professors.ClaudeShannonEmail,
                FullName: "Dr. Claude Shannon",
                Department: "Computer Engineering",
                Rank: "Full Professor",
                MaxCapacity: 3,
                AboutMe: "Focusing on hardware acceleration, IoT protocols, embedded C systems, ARM microcontrollers, FreeRTOS, and edge AI computation.",
                ResearchInterests: new[] { "Embedded Systems", "Internet of Things", "Edge AI", "ARM Architecture", "Microcontrollers" }
            ),

            // 5. Software Engineering / SQA & Compilers
            (
                Id: SeedConstants.Professors.GraceHopperId,
                Email: SeedConstants.Professors.GraceHopperEmail,
                FullName: "Dr. Grace Hopper",
                Department: "Software Engineering",
                Rank: "Full Professor",
                MaxCapacity: 10,
                AboutMe: "Expert in compiler design, programming language theory, automated software quality assurance, static analysis, and modern CI/CD automation pipelines.",
                ResearchInterests: new[] { "Compiler Design", "Software Quality Assurance", "CI/CD Automation", "Programming Languages", "Refactoring" }
            ),

            // 6. Game Development / 3D Graphics & Physics
            (
                Id: SeedConstants.Professors.JohnVonNeumannId,
                Email: SeedConstants.Professors.JohnVonNeumannEmail,
                FullName: "Dr. John von Neumann",
                Department: "Game Development & Interactive Media",
                Rank: "Associate Professor",
                MaxCapacity: 5,
                AboutMe: "Researching real-time physics rendering, Vulkan/OpenGL API graphics pipelines, 3D shader programming, and ray tracing engines.",
                ResearchInterests: new[] { "3D Computer Graphics", "Shader Programming", "Ray Tracing", "Real-Time Physics", "Game Engines" }
            ),

            // 7. Bioinformatics & Healthcare IT
            (
                Id: SeedConstants.Professors.RosalindFranklinId,
                Email: SeedConstants.Professors.RosalindFranklinEmail,
                FullName: "Dr. Rosalind Franklin",
                Department: "Bioinformatics & Health Informatics",
                Rank: "Assistant Professor",
                MaxCapacity: 4,
                AboutMe: "Researching genomic data sequence analysis, clinical data exchange standards (HL7/FHIR), medical image segmentation, and healthcare AI.",
                ResearchInterests: new[] { "Genomic Data Analysis", "Medical Imaging", "HL7 / FHIR Standards", "Computational Biology" }
            ),

            // 8. Quantum Computing Studies
            (
                Id: SeedConstants.Professors.RichardFeynmanId,
                Email: SeedConstants.Professors.RichardFeynmanEmail,
                FullName: "Dr. Richard Feynman",
                Department: "Theoretical Quantum Computing Studies",
                Rank: "Full Professor",
                MaxCapacity: 3,
                AboutMe: "Exploring quantum algorithm simulation, Qiskit execution frameworks, quantum key distribution cryptography, and fault-tolerant quantum error correction.",
                ResearchInterests: new[] { "Quantum Algorithms", "Qiskit Framework", "Quantum Cryptography", "Quantum Information" }
            ),

            // 9. Minimal Profile Edge Case (Testing optional UI details)
            (
                Id: SeedConstants.Professors.MargaretHamiltonId,
                Email: SeedConstants.Professors.MargaretHamiltonEmail,
                FullName: "Dr. Margaret Hamilton",
                Department: "Computer Science",
                Rank: "Assistant Professor",
                MaxCapacity: 5,
                AboutMe: "", // Minimal/empty bio edge case
                ResearchInterests: new[] { "Systems Engineering" }
            )
        };

        foreach (var profData in professorsToSeed)
        {
            // 1. Identity Account Provisioning
            var user = await userManager.FindByEmailAsync(profData.Email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    Id = profData.Id,
                    UserName = profData.Email,
                    Email = profData.Email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user, SeedConstants.DefaultPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, Roles.Professor);
                }
            }

            // 2. Domain Professor Profile Provisioning
            if (!await context.Professors.AnyAsync(p => p.Id == user.Id))
            {
                var professorProfile = new Professor(
                    id: user.Id,
                    fullName: profData.FullName,
                    department: profData.Department,
                    rank: profData.Rank,
                    maxSupervisionCapacity: profData.MaxCapacity
                );

                if (!string.IsNullOrWhiteSpace(profData.AboutMe))
                {
                    professorProfile.UpdateAboutMe(profData.AboutMe);
                }

                // 3. Attach Research Interests (Passing ResearchInterest.Id Guid)
                foreach (var area in profData.ResearchInterests)
                {
                    var researchInterestId = await GetOrCreateResearchInterestIdAsync(context, area);

                    // Crucial fix: Passing Guid ID instead of Entity instance
                    professorProfile.AddResearchInterest(researchInterestId);
                }

                await context.Professors.AddAsync(professorProfile);
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Helper method that looks up or creates a ResearchInterest entity and returns its Guid ID.
    /// </summary>
    private static async Task<Guid> GetOrCreateResearchInterestIdAsync(ApplicationDbContext context, string area)
    {
        var existingInterest = await context.Set<ResearchInterest>()
            .FirstOrDefaultAsync(ri => ri.Area == area);

        if (existingInterest != null)
        {
            return existingInterest.Id;
        }

        var newInterest = new ResearchInterest(area);
        await context.Set<ResearchInterest>().AddAsync(newInterest);
        await context.SaveChangesAsync();

        return newInterest.Id;
    }
}
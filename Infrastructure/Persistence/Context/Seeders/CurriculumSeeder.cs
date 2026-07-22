using AcademicGateway.Domain.Curriculum;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AcademicGateway.Infrastructure.Persistence.Context.Seeders;

/// <summary>
/// Seeder responsible for populating baseline academic curriculum entities (Majors and Specialties).
/// Includes diverse faculties, real-world specialized tracks, asymmetric specialty distributions,
/// special characters (&, /, -), long titles for UI stress-testing, and single/empty specialty edge cases.
/// </summary>
public static class CurriculumSeeder
{
    /// <summary>
    /// Evaluates and seeds default majors along with their corresponding specialties.
    /// </summary>
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Majors.AnyAsync())
        {
            return;
        }

        // 1. Computer Science
        var computerScience = new Major("Computer Science");
        computerScience.AddSpecialty("Cloud-Native Architecture & Distributed Systems");
        computerScience.AddSpecialty("Computer Vision & Graphics");
        computerScience.AddSpecialty("Theoretical Computer Science & Algorithms");
        computerScience.AddSpecialty("Compilers & Programming Languages");

        // 2. Software Engineering
        var softwareEngineering = new Major("Software Engineering");
        softwareEngineering.AddSpecialty("Full-Stack Web Systems");
        softwareEngineering.AddSpecialty("Mobile & Ubiquitous Computing");
        softwareEngineering.AddSpecialty("Software Quality Assurance & Automation");
        softwareEngineering.AddSpecialty("DevOps & Site Reliability Engineering (SRE)");

        // 3. Artificial Intelligence & Data Science (Special characters test: &, /)
        var aiAndDataScience = new Major("Artificial Intelligence & Data Science");
        aiAndDataScience.AddSpecialty("Machine Learning & Deep Learning");
        aiAndDataScience.AddSpecialty("Natural Language Processing (NLP) & LLMs");
        aiAndDataScience.AddSpecialty("Data Engineering & Big Data Infrastructure");
        aiAndDataScience.AddSpecialty("Autonomous Robotics / Perception");

        // 4. Cybersecurity & Network Infrastructure (Special characters test: &, -, commas)
        var cybersecurity = new Major("Cybersecurity & Network Infrastructure");
        cybersecurity.AddSpecialty("Offensive Security & Penetration Testing");
        cybersecurity.AddSpecialty("Zero-Trust Network Architecture");
        cybersecurity.AddSpecialty("Cryptography & Security Protocols");
        cybersecurity.AddSpecialty("Cloud, Container & Microservices Security");

        // 5. Computer Engineering
        var computerEngineering = new Major("Computer Engineering");
        computerEngineering.AddSpecialty("IoT & Embedded Systems");
        computerEngineering.AddSpecialty("VLSI Design & Microcontrollers");
        computerEngineering.AddSpecialty("Edge Computing & Hardware Acceleration");

        // 6. Information Systems & Digital Business
        var informationSystems = new Major("Information Systems & Digital Business");
        informationSystems.AddSpecialty("Enterprise Resource Planning (ERP)");
        informationSystems.AddSpecialty("Business Intelligence & Data Analytics");
        informationSystems.AddSpecialty("IT Governance, Risk & Compliance");

        // 7. Game Development & Interactive Media (Special characters test: /, ())
        var gameDev = new Major("Game Development & Interactive Media");
        gameDev.AddSpecialty("3D Game Engine Architecture");
        gameDev.AddSpecialty("Virtual Reality (VR) / Augmented Reality (AR)");
        gameDev.AddSpecialty("Real-Time Physics & Rendering Simulation");

        // 8. UI Edge Case: Ultra-Long String for layout, wrapping & truncation stress-testing
        var longMajorTrack = new Major("International Dual-Degree Advanced Autonomous Robotics & Intelligent Systems Engineering Track");
        longMajorTrack.AddSpecialty("Multi-Agent Swarm Intelligence & Distributed Autonomous Systems");
        longMajorTrack.AddSpecialty("High-Precision Multi-Sensor Spatial Fusion");

        // 9. Boundary Test: Major with exactly 1 Specialty
        var healthInformatics = new Major("Bioinformatics & Health Informatics");
        healthInformatics.AddSpecialty("Medical Data Analytics & Computational Genomics");

        // 10. Boundary Test: Unspecialized Major with 0 Specialties (Null/Empty specialty list testing for filter queries)
        var quantumComputing = new Major("Theoretical Quantum Computing Studies");

        await context.Majors.AddRangeAsync(
            computerScience,
            softwareEngineering,
            aiAndDataScience,
            cybersecurity,
            computerEngineering,
            informationSystems,
            gameDev,
            longMajorTrack,
            healthInformatics,
            quantumComputing
        );

        await context.SaveChangesAsync();
    }
}
using AcademicGateway.Domain.Common.Constants;
using AcademicGateway.Domain.Students;
using AcademicGateway.Infrastructure.Identity;
using Infrastructure.Persistence.Context.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AcademicGateway.Infrastructure.Persistence.Context.Seeders.DomainEntitySeeders;

/// <summary>
/// Seeder responsible for populating student user accounts and academic domain profiles.
/// Seeds diverse student personas across majors, specialties, graduation cohorts, 
/// skill stacks, and edge cases (double majors, incomplete profiles, long strings).
/// </summary>
public static class StudentSeeder
{
    /// <summary>
    /// Evaluates and seeds baseline student personas across disciplines.
    /// </summary>
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        // 1. Fetch Lookup Dictionary Data for Majors, Specialties, and Skills
        var majors = await context.Majors
            .Include(m => m.Specialties)
            .ToListAsync();

        var skills = await context.Skills
            .ToListAsync();

        var majorMap = majors.ToDictionary(m => m.Name, m => m, StringComparer.OrdinalIgnoreCase);
        var skillMap = skills.ToDictionary(s => s.Name, s => s, StringComparer.OrdinalIgnoreCase);

        // Helper function to resolve specialty GUID from major and specialty name
        Guid? GetSpecialtyId(string majorName, string specialtyName)
        {
            if (majorMap.TryGetValue(majorName, out var major))
            {
                var specialty = major.Specialties.FirstOrDefault(s => string.Equals(s.Name, specialtyName, StringComparison.OrdinalIgnoreCase));
                return specialty?.Id;
            }
            return null;
        }

        var studentsToSeed = new[]
        {
            // 1. Jane Doe - Standard CS / Software Engineering
            new
            {
                Id = SeedConstants.Students.JaneDoeId,
                Email = SeedConstants.Students.JaneDoeEmail,
                FullName = "Jane Doe",
                GraduationYear = 2027,
                AboutMe = "Final-year Computer Science student specializing in Software Engineering, interested in backend C# development and cloud architecture.",
                Majors = new[] { "Computer Science" },
                Specialties = new[] { ("Computer Science", "Cloud-Native Architecture & Distributed Systems") },
                Skills = new[] { "C# .NET Backend Development", "ASP.NET Core Web API", "Docker Containerization", "PostgreSQL Database Design", "React TypeScript Frontend" }
            },

            // 2. John Smith - Full-Stack Web Senior
            new
            {
                Id = SeedConstants.Students.JohnSmithId,
                Email = SeedConstants.Students.JohnSmithEmail,
                FullName = "John Smith",
                GraduationYear = 2026,
                AboutMe = "Graduating Software Engineering senior passionate about full-stack web applications, modern JavaScript frameworks, and RESTful APIs.",
                Majors = new[] { "Software Engineering" },
                Specialties = new[] { ("Software Engineering", "Full-Stack Web Systems") },
                Skills = new[] { "React TypeScript Frontend", "Node.js & Asynchronous Systems", "RESTful API & GraphQL Design", "Agile, Scrum & Kanban Frameworks" }
            },

            // 3. Alice Johnson - AI & Data Science Track
            new
            {
                Id = SeedConstants.Students.AliceJohnsonId,
                Email = SeedConstants.Students.AliceJohnsonEmail,
                FullName = "Alice Johnson",
                GraduationYear = 2027,
                AboutMe = "Data science enthusiast focused on deep learning, neural networks, natural language processing, and predictive analytics.",
                Majors = new[] { "Artificial Intelligence & Data Science" },
                Specialties = new[] { ("Artificial Intelligence & Data Science", "Machine Learning & Deep Learning") },
                Skills = new[] { "Python Machine Learning", "PyTorch & Deep Learning", "Natural Language Processing (NLP) & LLMs", "SQL & Relational Modeling" }
            },

            // 4. Bob Williams - Cybersecurity Track
            new
            {
                Id = SeedConstants.Students.BobWilliamsId,
                Email = SeedConstants.Students.BobWilliamsEmail,
                FullName = "Bob Williams",
                GraduationYear = 2028,
                AboutMe = "Junior cybersecurity researcher specializing in ethical hacking, penetration testing, zero-trust network defenses, and threat analysis.",
                Majors = new[] { "Cybersecurity & Network Infrastructure" },
                Specialties = new[] { ("Cybersecurity & Network Infrastructure", "Offensive Security & Penetration Testing") },
                Skills = new[] { "Penetration Testing & Ethical Hacking", "Network Security & Threat Modeling", "Cryptography & Security Protocols" }
            },

            // 5. Charlie Brown - Computer Engineering & IoT Track
            new
            {
                Id = SeedConstants.Students.CharlieBrownId,
                Email = SeedConstants.Students.CharlieBrownEmail,
                FullName = "Charlie Brown",
                GraduationYear = 2026,
                AboutMe = "Hardware and systems developer interested in microcontrollers, embedded C, micro-sensors, and real-time operating systems.",
                Majors = new[] { "Computer Engineering" },
                Specialties = new[] { ("Computer Engineering", "IoT & Embedded Systems") },
                Skills = new[] { "C Systems Programming", "Microcontrollers & Embedded C", "Internet of Things (IoT) Protocols" }
            },

            // 6. Eva Martinez - Game Development & Graphics Track
            new
            {
                Id = SeedConstants.Students.EvaMartinezId,
                Email = SeedConstants.Students.EvaMartinezEmail,
                FullName = "Eva Martinez",
                GraduationYear = 2027,
                AboutMe = "Interactive media developer specializing in 3D game engines, real-time physics rendering, shader optimization, and Unity/Unreal mechanics.",
                Majors = new[] { "Game Development & Interactive Media" },
                Specialties = new[] { ("Game Development & Interactive Media", "3D Game Engine Architecture") },
                Skills = new[] { "Unity Engine & C# Scripting", "Unreal Engine & C++", "3D Computer Graphics & Shader Programming" }
            },

            // 7. David Lee - Bioinformatics & Health IT Track
            new
            {
                Id = SeedConstants.Students.DavidLeeId,
                Email = SeedConstants.Students.DavidLeeEmail,
                FullName = "David Lee",
                GraduationYear = 2026,
                AboutMe = "Computational biology student working with genomic sequencing pipelines, medical image processing, and HL7 health data standards.",
                Majors = new[] { "Bioinformatics & Health Informatics" },
                Specialties = new[] { ("Bioinformatics & Health Informatics", "Medical Data Analytics & Computational Genomics") },
                Skills = new[] { "Genomic Data Analysis", "HL7 / FHIR Health Data Standards", "Python Machine Learning" }
            },

            // 8. Sophia Chen - Double Major & Heavy Skill Stack (UI Tag Overflow Edge Case)
            new
            {
                Id = SeedConstants.Students.SophiaChenId,
                Email = SeedConstants.Students.SophiaChenEmail,
                FullName = "Sophia Chen",
                GraduationYear = 2027,
                AboutMe = "Double major in Computer Science and AI, exploring intelligent cloud-native architectures, distributed ML pipelines, and SRE best practices.",
                Majors = new[] { "Computer Science", "Artificial Intelligence & Data Science" },
                Specialties = new[] {
                    ("Computer Science", "Cloud-Native Architecture & Distributed Systems"),
                    ("Artificial Intelligence & Data Science", "Data Engineering & Big Data Infrastructure")
                },
                Skills = new[] {
                    "C# .NET Backend Development", "ASP.NET Core Web API", "Docker Containerization",
                    "Kubernetes & Orchestration", "AWS Cloud Infrastructure", "CI/CD Automation Pipelines",
                    "Python Machine Learning", "Data Engineering & Apache Spark", "PostgreSQL Database Design",
                    "Agile, Scrum & Kanban Frameworks"
                }
            },

            // 9. Liam Wilson - Minimal / Incomplete Profile Edge Case
            new
            {
                Id = SeedConstants.Students.LiamWilsonId,
                Email = SeedConstants.Students.LiamWilsonEmail,
                FullName = "Liam Wilson",
                GraduationYear = 2028,
                AboutMe = "", // Minimal profile / no bio
                Majors = Array.Empty<string>(), // No major selected yet
                Specialties = Array.Empty<(string Major, string Specialty)>(),
                Skills = Array.Empty<string>() // No skills added yet
            },

            // 10. Alexander - Long Name String Edge Case (UI layout testing)
            new
            {
                Id = SeedConstants.Students.AlexanderHohenzollernId,
                Email = SeedConstants.Students.AlexanderHohenzollernEmail,
                FullName = "Alexander Maximilian-Montgomery von Hohenzollern the Third",
                GraduationYear = 2027,
                AboutMe = "Advanced autonomous systems engineer interested in multi-agent swarm robotics and spatial perception.",
                Majors = new[] { "International Dual-Degree Advanced Autonomous Robotics & Intelligent Systems Engineering Track" },
                Specialties = new[] { ("International Dual-Degree Advanced Autonomous Robotics & Intelligent Systems Engineering Track", "Multi-Agent Swarm Intelligence & Distributed Autonomous Systems") },
                Skills = new[] { "ROS / ROS2 (Robot Operating System)", "Computer Vision & OpenCV", "Sensor Fusion & Spatial Perception" }
            }
        };

        foreach (var studentData in studentsToSeed)
        {
            // 1. Identity Account Provisioning
            var user = await userManager.FindByEmailAsync(studentData.Email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    Id = studentData.Id,
                    UserName = studentData.Email,
                    Email = studentData.Email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user, SeedConstants.DefaultPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, Roles.Student);
                }
            }

            // 2. Domain Student Profile Provisioning
            if (!await context.Students.AnyAsync(s => s.Id == user.Id))
            {
                var studentProfile = new Student(
                    id: user.Id,
                    fullName: studentData.FullName,
                    graduationYear: studentData.GraduationYear
                );

                if (!string.IsNullOrWhiteSpace(studentData.AboutMe))
                {
                    studentProfile.UpdateAboutMe(studentData.AboutMe);
                }

                // Attach Majors
                foreach (var majorName in studentData.Majors)
                {
                    if (majorMap.TryGetValue(majorName, out var majorEntity))
                    {
                        studentProfile.AddMajor(majorEntity.Id);
                    }
                }

                // Attach Specialties
                foreach (var (mName, sName) in studentData.Specialties)
                {
                    var specialtyId = GetSpecialtyId(mName, sName);
                    if (specialtyId.HasValue)
                    {
                        studentProfile.AddSpecialty(specialtyId.Value);
                    }
                }

                // Attach Skills
                foreach (var skillName in studentData.Skills)
                {
                    if (skillMap.TryGetValue(skillName, out var skillEntity))
                    {
                        studentProfile.AddSkill(skillEntity.Id);
                    }
                }

                await context.Students.AddAsync(studentProfile);
            }
        }

        await context.SaveChangesAsync();
    }
}
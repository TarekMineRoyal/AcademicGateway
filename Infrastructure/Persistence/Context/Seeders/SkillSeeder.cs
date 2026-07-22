using AcademicGateway.Domain.Skills;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcademicGateway.Infrastructure.Persistence.Context.Seeders;

/// <summary>
/// Seeder responsible for populating baseline platform skills.
/// Seeds a rich dictionary of technical, domain-specific, methodology,
/// and soft skills to support student profiling, project tagging, and AI matchmaking.
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
                // 1. Software Engineering & Web Development
                new("C# .NET Backend Development"),
                new("ASP.NET Core Web API"),
                new("React TypeScript Frontend"),
                new("Node.js & Asynchronous Systems"),
                new("RESTful API & GraphQL Design"),
                new("Software Testing & Quality Assurance (SQA)"),
                new("Microservices Architecture"),

                // 2. Cloud Computing, DevOps & Infrastructure
                new("Docker Containerization"),
                new("Kubernetes & Orchestration"),
                new("AWS Cloud Infrastructure"),
                new("Azure Cloud Services"),
                new("CI/CD Automation Pipelines"),
                new("Terraform & Infrastructure as Code (IaC)"),

                // 3. Artificial Intelligence, Data & Analytics
                new("Python Machine Learning"),
                new("PyTorch & Deep Learning"),
                new("Natural Language Processing (NLP) & LLMs"),
                new("Data Engineering & Apache Spark"),
                new("PostgreSQL Database Design"),
                new("SQL & Relational Modeling"),
                new("NoSQL & Document Databases"),

                // 4. Cybersecurity & Systems Security
                new("Penetration Testing & Ethical Hacking"),
                new("Network Security & Threat Modeling"),
                new("Cryptography & Security Protocols"),
                new("Identity & Access Management (IAM)"),

                // 5. Embedded Systems, IoT & Hardware
                new("C Systems Programming"),
                new("C++ Low-Level Development"),
                new("Microcontrollers & Embedded C"),
                new("Internet of Things (IoT) Protocols"),
                new("Verilog & FPGA Hardware Design"),

                // 6. Game Development, Graphics & Interactive Media
                new("Unity Engine & C# Scripting"),
                new("Unreal Engine & C++"),
                new("3D Computer Graphics & Shader Programming"),
                new("Virtual Reality (VR) / Augmented Reality (AR)"),

                // 7. Robotics, Autonomous Systems & Quantum Computing
                new("ROS / ROS2 (Robot Operating System)"),
                new("Computer Vision & OpenCV"),
                new("Sensor Fusion & Spatial Perception"),
                new("Quantum Computing & Qiskit"),

                // 8. Bioinformatics & Healthcare IT
                new("Genomic Data Analysis"),
                new("HL7 / FHIR Health Data Standards"),

                // 9. Product, Design & Business Methodology
                new("UI/UX Design & Figma Prototyping"),
                new("Agile, Scrum & Kanban Frameworks"),
                new("Technical Product Management"),
                new("Business Process Modeling"),

                // 10. Universal Soft Skills & Professional Competencies
                new("Critical Thinking & Analytical Problem Solving"),
                new("Technical Documentation & Spec Writing"),
                new("Public Speaking & Technical Presentation"),
                new("Cross-Functional Team Collaboration"),
                new("Academic Research & Literature Review"),

                // 11. UI Edge Case: Ultra-long string test
                new("Ethical Artificial Intelligence, Algorithmic Bias & Technology Governance"),

                // 12. Short Acronyms / Exact Search Match Edge Cases
                new("AI"),
                new("ROS"),
                new("SQL"),
                new("AWS"),
                new("C")
            };

            await context.Skills.AddRangeAsync(defaultSkills);
            await context.SaveChangesAsync();
        }
    }
}
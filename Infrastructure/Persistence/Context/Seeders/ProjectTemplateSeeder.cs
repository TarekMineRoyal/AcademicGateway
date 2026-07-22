using AcademicGateway.Domain.Common.Enums;
using AcademicGateway.Domain.ProjectTemplates;
using Infrastructure.Persistence.Context.Seeders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AcademicGateway.Infrastructure.Persistence.Context.Seeders;

/// <summary>
/// Seeder responsible for populating baseline project templates, global milestones, 
/// tasks, milestone dependencies, required skills, and academic major/specialty bindings.
/// Seeds templates testing diverse Work Breakdown Structure (WBS) graph topologies 
/// (Linear Cascade, Diamond DAG, Disjoint Tracks, Single Task, Asymmetric Weights, Empty Milestones).
/// </summary>
public static class ProjectTemplateSeeder
{
    /// <summary>
    /// Evaluates and seeds baseline project templates across approval lifecycle states.
    /// </summary>
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.ProjectTemplates.AnyAsync())
        {
            return;
        }

        // 1. Fetch Lookup Dictionary Data for Majors, Specialties, and Skills
        var majors = await context.Majors
            .Include(m => m.Specialties)
            .ToListAsync();

        var skills = await context.Skills
            .ToListAsync();

        var majorMap = majors.ToDictionary(m => m.Name, m => m, StringComparer.OrdinalIgnoreCase);
        var skillMap = skills.ToDictionary(s => s.Name, s => s, StringComparer.OrdinalIgnoreCase);

        // Helper functions
        Guid? GetMajorId(string majorName)
        {
            return majorMap.TryGetValue(majorName, out var major) ? major.Id : null;
        }

        Guid? GetSpecialtyId(string majorName, string specialtyName)
        {
            if (majorMap.TryGetValue(majorName, out var major))
            {
                var specialty = major.Specialties.FirstOrDefault(s => string.Equals(s.Name, specialtyName, StringComparison.OrdinalIgnoreCase));
                return specialty?.Id;
            }
            return null;
        }

        void AttachSkills(ProjectTemplate template, params string[] skillNames)
        {
            foreach (var name in skillNames)
            {
                if (skillMap.TryGetValue(name, out var skill))
                {
                    template.AddSkill(skill.Id);
                }
            }
        }

        // =========================================================================
        // --- Template 1: Distributed E-Commerce Cloud Migration ---
        // Status: Approved
        // Topology: Linear Cascade Chain (M1 -> M2 -> M3)
        // Track: Computer Science / Cloud-Native Architecture
        // =========================================================================
        var cloudTemplate = new ProjectTemplate(
            title: "Distributed E-Commerce Cloud Infrastructure Migration",
            description: "Design and implement a fully automated microservices deployment pipeline using Docker, Kubernetes, and PostgreSQL backend clusters.",
            providerId: SeedConstants.Providers.AcmeCorpId,
            majorId: GetMajorId("Computer Science"),
            specialtyId: GetSpecialtyId("Computer Science", "Cloud-Native Architecture & Distributed Systems"),
            createdAt: DateTime.UtcNow.AddDays(-10)
        );

        AttachSkills(cloudTemplate, "C# .NET Backend Development", "Docker Containerization", "PostgreSQL Database Design", "RESTful API & GraphQL Design");

        // Milestone 1 (WBS: 20%, Grade: 20%)
        cloudTemplate.AddMilestone("Research & Architecture Specification", "Analyze system bottlenecks and formulate a cloud migration roadmap.", 15.5m, 20.00m, 20.00m);
        var cloudM1 = cloudTemplate.GlobalMilestones.First(m => m.Title == "Research & Architecture Specification");
        cloudTemplate.AddGlobalTaskToMilestone(cloudM1.Id, "Literature & Patterns Review", "Evaluate containerization and cluster replication topologies.", 50.00m, DeliverableType.File);
        cloudTemplate.AddGlobalTaskToMilestone(cloudM1.Id, "Use Case & Schema Specification", "Outline data durability scenarios and network bounds.", 50.00m, DeliverableType.Text);

        // Milestone 2 (WBS: 50%, Grade: 50%)
        cloudTemplate.AddMilestone("Core Microservices Deployment", "Deploy production-ready containerized microservices and database replica pods.", 35.0m, 50.00m, 50.00m);
        var cloudM2 = cloudTemplate.GlobalMilestones.First(m => m.Title == "Core Microservices Deployment");
        cloudTemplate.AddGlobalTaskToMilestone(cloudM2.Id, "API Services Construction", "Construct robust C# microservice endpoints.", 60.00m, DeliverableType.Url);
        cloudTemplate.AddGlobalTaskToMilestone(cloudM2.Id, "Database Cluster Setup", "Provision high-availability PostgreSQL clusters.", 40.00m, DeliverableType.Url);

        // Milestone 3 (WBS: 30%, Grade: 30%)
        cloudTemplate.AddMilestone("Telemetry & Final Thesis Defense", "Instrument health monitoring daemons and compile structural defense documentation.", 25.0m, 30.00m, 30.00m);
        var cloudM3 = cloudTemplate.GlobalMilestones.First(m => m.Title == "Telemetry & Final Thesis Defense");
        cloudTemplate.AddGlobalTaskToMilestone(cloudM3.Id, "Prometheus Telemetry Instrumentation", "Embed performance tracking daemons.", 50.00m, DeliverableType.Url);
        cloudTemplate.AddGlobalTaskToMilestone(cloudM3.Id, "Final Capstone Thesis Upload", "Publish finalized project codebase metrics.", 50.00m, DeliverableType.File);

        // Dependencies
        cloudTemplate.AddMilestoneDependency(cloudM2.Id, cloudM1.Id, DependencyType.FinishToStart);
        cloudTemplate.AddMilestoneDependency(cloudM3.Id, cloudM2.Id, DependencyType.FinishToStart);

        cloudTemplate.SubmitForReview();
        cloudTemplate.Approve();
        await context.ProjectTemplates.AddAsync(cloudTemplate);

        // =========================================================================
        // --- Template 2: Enterprise Multi-Region Cloud Infra ---
        // Status: Approved
        // Topology: Complex Diamond DAG (7 Milestones, 14 Tasks, 7 Dependencies)
        // Track: Computer Science / Cloud-Native Architecture
        // =========================================================================
        var infraTemplate = new ProjectTemplate(
            title: "Enterprise Multi-Region Infrastructure Layer Orchestration",
            description: "Design, build, and evaluate a resilient multi-region core infrastructure platform comprising secure VPC topologies, automated state databases, load-balanced compute node fleets, GitOps deployment automation, and telemetry tracing pipelines.",
            providerId: SeedConstants.Providers.CloudSystemsId,
            majorId: GetMajorId("Computer Science"),
            specialtyId: GetSpecialtyId("Computer Science", "Cloud-Native Architecture & Distributed Systems"),
            createdAt: DateTime.UtcNow.AddDays(-15)
        );

        AttachSkills(infraTemplate, "Kubernetes & Orchestration", "AWS Cloud Infrastructure", "CI/CD Automation Pipelines", "Terraform & Infrastructure as Code (IaC)", "Docker Containerization");

        infraTemplate.AddMilestone("Infrastructure Base Network & Security Provisioning", "Establish base cloud networks and IAM boundary access rules.", 20.0m, 15.00m, 10.00m);
        var infraM1 = infraTemplate.GlobalMilestones.First(m => m.Title == "Infrastructure Base Network & Security Provisioning");
        infraTemplate.AddGlobalTaskToMilestone(infraM1.Id, "VPC & Subnet Topologies Setup", "Configure multi-region virtual private spaces.", 50.00m, DeliverableType.Url);
        infraTemplate.AddGlobalTaskToMilestone(infraM1.Id, "IAM Access Control Rules", "Formulate security roles and boundary access levels.", 50.00m, DeliverableType.File);

        infraTemplate.AddMilestone("Distributed Shared Storage & State Matrix Definition", "Deploy persistent data stores and cluster replication topologies.", 25.0m, 15.00m, 10.00m);
        var infraM2 = infraTemplate.GlobalMilestones.First(m => m.Title == "Distributed Shared Storage & State Matrix Definition");
        infraTemplate.AddGlobalTaskToMilestone(infraM2.Id, "Database Cluster Replication Setup", "Provision high-availability engines.", 50.00m, DeliverableType.Url);
        infraTemplate.AddGlobalTaskToMilestone(infraM2.Id, "State Snapshot Verification", "Build metrics to capture backup storage pipeline resilience.", 50.00m, DeliverableType.Text);

        infraTemplate.AddMilestone("Container Fleet Clusters Provisioning", "Initialize target platform orchestration nodes.", 30.0m, 10.00m, 10.00m);
        var infraM3 = infraTemplate.GlobalMilestones.First(m => m.Title == "Container Fleet Clusters Provisioning");
        infraTemplate.AddGlobalTaskToMilestone(infraM3.Id, "Compute Control Plane Configuration", "Instantiate orchestration managers.", 50.00m, DeliverableType.Url);
        infraTemplate.AddGlobalTaskToMilestone(infraM3.Id, "Dynamic Scaling Policy Scripts", "Establish baseline boundaries to balance workload execution nodes.", 50.00m, DeliverableType.File);

        infraTemplate.AddMilestone("Routing Traffic Proxies & Gateway Configuration", "Configure layer-7 application load balancers and edge firewalls.", 35.0m, 20.00m, 25.00m);
        var infraM4 = infraTemplate.GlobalMilestones.First(m => m.Title == "Routing Traffic Proxies & Gateway Configuration");
        infraTemplate.AddGlobalTaskToMilestone(infraM4.Id, "Ingress Controller Implementation", "Expose secure transit boundaries.", 50.00m, DeliverableType.Url);
        infraTemplate.AddGlobalTaskToMilestone(infraM4.Id, "Edge Firewall Security Hardening", "Bind TLS certificates and configure throttling rules.", 50.00m, DeliverableType.File);

        infraTemplate.AddMilestone("CI/CD GitOps Continuous Deployment Pipeline", "Link repository webhooks to continuous automated verification systems.", 40.0m, 15.00m, 15.00m);
        var infraM5 = infraTemplate.GlobalMilestones.First(m => m.Title == "CI/CD GitOps Continuous Deployment Pipeline");
        infraTemplate.AddGlobalTaskToMilestone(infraM5.Id, "GitOps Engine Design", "Construct declarative pipeline configurations.", 50.00m, DeliverableType.File);
        infraTemplate.AddGlobalTaskToMilestone(infraM5.Id, "Automated Promotion Mapping", "Integrate automated health check validation triggers.", 50.00m, DeliverableType.Url);

        infraTemplate.AddMilestone("Central Logging & Monitoring Observability Matrix", "Deploy log aggregators and collectors to capture telemetry patterns.", 20.0m, 12.00m, 15.00m);
        var infraM6 = infraTemplate.GlobalMilestones.First(m => m.Title == "Central Logging & Monitoring Observability Matrix");
        infraTemplate.AddGlobalTaskToMilestone(infraM6.Id, "Telemetry Agent Instrumentation", "Embed performance tracking daemons.", 50.00m, DeliverableType.Url);
        infraTemplate.AddGlobalTaskToMilestone(infraM6.Id, "Alert Routing Configurations", "Formulate system breach alerts.", 50.00m, DeliverableType.Text);

        infraTemplate.AddMilestone("Disaster Recovery Testing & Governance Report", "Conduct active network disruption chaos assessments.", 25.0m, 13.00m, 15.00m);
        var infraM7 = infraTemplate.GlobalMilestones.First(m => m.Title == "Disaster Recovery Testing & Governance Report");
        infraTemplate.AddGlobalTaskToMilestone(infraM7.Id, "Chaos Engineering Failure Simulation", "Execute live cluster partition scenarios.", 50.00m, DeliverableType.File);
        infraTemplate.AddGlobalTaskToMilestone(infraM7.Id, "Architectural Performance Summary", "Compile performance summary documenting network metrics.", 50.00m, DeliverableType.File);

        // DAG Dependencies
        infraTemplate.AddMilestoneDependency(infraM3.Id, infraM1.Id, DependencyType.StartToStart);
        infraTemplate.AddMilestoneDependency(infraM4.Id, infraM1.Id, DependencyType.FinishToStart);
        infraTemplate.AddMilestoneDependency(infraM4.Id, infraM3.Id, DependencyType.FinishToStart);
        infraTemplate.AddMilestoneDependency(infraM5.Id, infraM2.Id, DependencyType.StartToStart);
        infraTemplate.AddMilestoneDependency(infraM5.Id, infraM3.Id, DependencyType.StartToStart);
        infraTemplate.AddMilestoneDependency(infraM6.Id, infraM5.Id, DependencyType.FinishToStart);
        infraTemplate.AddMilestoneDependency(infraM7.Id, infraM5.Id, DependencyType.FinishToStart);

        infraTemplate.SubmitForReview();
        infraTemplate.Approve();
        await context.ProjectTemplates.AddAsync(infraTemplate);

        // =========================================================================
        // --- Template 3: Full-Stack Autonomous Swarm Robotics ---
        // Status: Approved
        // Topology: Disjoint Dual Tracks (Track A: Hardware, Track B: AI, Converging in M5)
        // Task Granularity: Fine-grained odd decimal task weights (10%, 15%, 25%, 20%, 30%)
        // Track: International Dual-Degree Advanced Autonomous Robotics...
        // =========================================================================
        var roboticsTemplate = new ProjectTemplate(
            title: "Multi-Agent Swarm Robotics & Spatial Perception Engine",
            description: "Build an integrated hardware-software swarm robotics platform utilizing ROS2, micro-controllers, spatial sensor fusion, and decentralised obstacle avoidance algorithms.",
            providerId: SeedConstants.Providers.AcmeCorpId,
            majorId: GetMajorId("International Dual-Degree Advanced Autonomous Robotics & Intelligent Systems Engineering Track"),
            specialtyId: GetSpecialtyId("International Dual-Degree Advanced Autonomous Robotics & Intelligent Systems Engineering Track", "Multi-Agent Swarm Intelligence & Distributed Autonomous Systems"),
            createdAt: DateTime.UtcNow.AddDays(-8)
        );

        AttachSkills(roboticsTemplate, "ROS / ROS2 (Robot Operating System)", "Computer Vision & OpenCV", "Sensor Fusion & Spatial Perception", "Microcontrollers & Embedded C", "C++ Low-Level Development");

        // Track A: Hardware
        roboticsTemplate.AddMilestone("Hardware PCB & Micro-Sensor Fabrication", "Design micro-controller boards and solder sensor arrays.", 30.0m, 20.00m, 15.00m);
        var robM1 = roboticsTemplate.GlobalMilestones.First(m => m.Title == "Hardware PCB & Micro-Sensor Fabrication");
        roboticsTemplate.AddGlobalTaskToMilestone(robM1.Id, "Schematic Layout Drafting", "Draft hardware schematic diagrams.", 10.00m, DeliverableType.File);
        roboticsTemplate.AddGlobalTaskToMilestone(robM1.Id, "Sensor Pin Calibration", "Calibrate IMU and LiDAR pin assignments.", 15.00m, DeliverableType.Text);
        roboticsTemplate.AddGlobalTaskToMilestone(robM1.Id, "Microcontroller Firmware Flashing", "Flash embedded C bootloaders.", 25.00m, DeliverableType.File);
        roboticsTemplate.AddGlobalTaskToMilestone(robM1.Id, "Power Management Circuit Test", "Verify battery regulation and heat output.", 20.00m, DeliverableType.Text);
        roboticsTemplate.AddGlobalTaskToMilestone(robM1.Id, "Hardware Integration Verification", "Execute bench diagnostics on assembled units.", 30.00m, DeliverableType.Url);

        roboticsTemplate.AddMilestone("Low-Level Drivers & Motor Controller Firmware", "Implement motor PWM signals and IMU telemetry drivers.", 25.0m, 20.00m, 20.00m);
        var robM2 = roboticsTemplate.GlobalMilestones.First(m => m.Title == "Low-Level Drivers & Motor Controller Firmware");
        roboticsTemplate.AddGlobalTaskToMilestone(robM2.Id, "PWM Signal Tuning", "Tune motor speed controllers.", 50.00m, DeliverableType.File);
        roboticsTemplate.AddGlobalTaskToMilestone(robM2.Id, "IMU Data Serial Driver", "Build C driver for IMU data streams.", 50.00m, DeliverableType.Url);

        // Track B: AI Engine
        roboticsTemplate.AddMilestone("ROS2 Swarm Perception & Spatial Vision", "Build point-cloud spatial mapping and object detection nodes.", 30.0m, 20.00m, 15.00m);
        var robM3 = roboticsTemplate.GlobalMilestones.First(m => m.Title == "ROS2 Swarm Perception & Spatial Vision");
        roboticsTemplate.AddGlobalTaskToMilestone(robM3.Id, "OpenCV Point-Cloud Pipeline", "Process camera frames for depth mapping.", 50.00m, DeliverableType.Url);
        roboticsTemplate.AddGlobalTaskToMilestone(robM3.Id, "Spatial Sensor Fusion Node", "Combine IMU and visual odometry data.", 50.00m, DeliverableType.File);

        roboticsTemplate.AddMilestone("Decentralized Swarm Consensus Algorithm", "Formulate consensus logic for dynamic leaderless formation navigation.", 35.0m, 20.00m, 20.00m);
        var robM4 = roboticsTemplate.GlobalMilestones.First(m => m.Title == "Decentralized Swarm Consensus Algorithm");
        roboticsTemplate.AddGlobalTaskToMilestone(robM4.Id, "Flocking & Collision Avoidance Logic", "Implement Reynolds flocking rules.", 50.00m, DeliverableType.File);
        roboticsTemplate.AddGlobalTaskToMilestone(robM4.Id, "Peer-to-Peer Mesh Telemetry", "Broadcast state positions across agents.", 50.00m, DeliverableType.Url);

        // Convergence Node
        roboticsTemplate.AddMilestone("Physical Field Demonstration & Defense Report", "Execute multi-robot swarm field tests and compile experimental logs.", 40.0m, 20.00m, 30.00m);
        var robM5 = roboticsTemplate.GlobalMilestones.First(m => m.Title == "Physical Field Demonstration & Defense Report");
        roboticsTemplate.AddGlobalTaskToMilestone(robM5.Id, "Live Swarm Arena Video Recording", "Capture live physical obstacle navigation video.", 40.00m, DeliverableType.Url);
        roboticsTemplate.AddGlobalTaskToMilestone(robM5.Id, "Final Research Thesis Document", "Submit final experimental thesis paper.", 60.00m, DeliverableType.File);

        // Dependencies
        roboticsTemplate.AddMilestoneDependency(robM2.Id, robM1.Id, DependencyType.FinishToStart); // Track A
        roboticsTemplate.AddMilestoneDependency(robM4.Id, robM3.Id, DependencyType.FinishToStart); // Track B
        roboticsTemplate.AddMilestoneDependency(robM5.Id, robM2.Id, DependencyType.FinishToStart); // Convergence from A
        roboticsTemplate.AddMilestoneDependency(robM5.Id, robM4.Id, DependencyType.FinishToStart); // Convergence from B

        roboticsTemplate.SubmitForReview();
        roboticsTemplate.Approve();
        await context.ProjectTemplates.AddAsync(roboticsTemplate);

        // =========================================================================
        // --- Template 4: Zero-Trust Cybersecurity Penetration Lab ---
        // Status: Approved
        // Topology: Concurrent Start (M2 and M3 StartToStart with M1)
        // Feature: Asymmetric WBS Weight (60%) vs. Grading Weight (15%)
        // Track: Cybersecurity & Network Infrastructure
        // =========================================================================
        var securityTemplate = new ProjectTemplate(
            title: "Zero-Trust Container Security & Penetration Testing Suite",
            description: "Develop automated vulnerability scanning engines, design zero-trust network boundaries, and conduct live ethical hacking penetration exercises.",
            providerId: SeedConstants.Providers.CyberShieldId,
            majorId: GetMajorId("Cybersecurity & Network Infrastructure"),
            specialtyId: GetSpecialtyId("Cybersecurity & Network Infrastructure", "Offensive Security & Penetration Testing"),
            createdAt: DateTime.UtcNow.AddDays(-12)
        );

        AttachSkills(securityTemplate, "Penetration Testing & Ethical Hacking", "Network Security & Threat Modeling", "Cryptography & Security Protocols", "Identity & Access Management (IAM)");

        securityTemplate.AddMilestone("Threat Modeling & Boundary Definition", "Perform risk assessments on microservice API gateways.", 15.0m, 10.00m, 10.00m);
        var secM1 = securityTemplate.GlobalMilestones.First(m => m.Title == "Threat Modeling & Boundary Definition");
        securityTemplate.AddGlobalTaskToMilestone(secM1.Id, "STRIDE Threat Matrix", "Catalog attack vectors and risk profiles.", 50.00m, DeliverableType.File);
        securityTemplate.AddGlobalTaskToMilestone(secM1.Id, "Network Topology Diagramming", "Diagram zero-trust perimeter zones.", 50.00m, DeliverableType.File);

        // Asymmetric Milestone (Takes 60% of WBS effort, but worth 15% of Grade)
        securityTemplate.AddMilestone("Heavy Vulnerability Scanning Execution", "Run long-duration brute force and fuzzing workloads across simulated clusters.", 60.0m, 60.00m, 15.00m);
        var secM2 = securityTemplate.GlobalMilestones.First(m => m.Title == "Heavy Vulnerability Scanning Execution");
        securityTemplate.AddGlobalTaskToMilestone(secM2.Id, "Fuzzing Harness Scripting", "Write dynamic buffer overflow fuzzers.", 50.00m, DeliverableType.Url);
        securityTemplate.AddGlobalTaskToMilestone(secM2.Id, "CVE Scans Log Aggregation", "Collect scan metrics across 1,000 endpoint tests.", 50.00m, DeliverableType.Text);

        securityTemplate.AddMilestone("Cryptographic PKI & IAM Implementation", "Deploy public key infrastructure and token authentication gateways.", 25.0m, 15.00m, 35.00m);
        var secM3 = securityTemplate.GlobalMilestones.First(m => m.Title == "Cryptographic PKI & IAM Implementation");
        securityTemplate.AddGlobalTaskToMilestone(secM3.Id, "Elliptic Curve Certificate Authority", "Construct private root CA service.", 50.00m, DeliverableType.Url);
        securityTemplate.AddGlobalTaskToMilestone(secM3.Id, "OAuth2 / mTLS Token Gateway", "Implement mutual TLS authentication proxies.", 50.00m, DeliverableType.Url);

        securityTemplate.AddMilestone("Penetration Audit Final Report", "Compile red-team penetration testing findings and remediation recommendations.", 20.0m, 15.00m, 40.00m);
        var secM4 = securityTemplate.GlobalMilestones.First(m => m.Title == "Penetration Audit Final Report");
        securityTemplate.AddGlobalTaskToMilestone(secM4.Id, "Red-Team Incident Log Document", "Document exploited entry points.", 50.00m, DeliverableType.File);
        securityTemplate.AddGlobalTaskToMilestone(secM4.Id, "Hardening Guidelines Thesis", "Formulate long-term remediation standards.", 50.00m, DeliverableType.File);

        // Dependencies: M2 and M3 start concurrently alongside M1
        securityTemplate.AddMilestoneDependency(secM2.Id, secM1.Id, DependencyType.StartToStart);
        securityTemplate.AddMilestoneDependency(secM3.Id, secM1.Id, DependencyType.StartToStart);
        securityTemplate.AddMilestoneDependency(secM4.Id, secM2.Id, DependencyType.FinishToStart);
        securityTemplate.AddMilestoneDependency(secM4.Id, secM3.Id, DependencyType.FinishToStart);

        securityTemplate.SubmitForReview();
        securityTemplate.Approve();
        await context.ProjectTemplates.AddAsync(securityTemplate);

        // =========================================================================
        // --- Template 5: Real-Time 3D Vulkan Shader Engine ---
        // Status: Approved
        // Feature: Single-Task Granularity (Each milestone contains exactly 1 task = 100%)
        // Track: Game Development & Interactive Media
        // =========================================================================
        var gameTemplate = new ProjectTemplate(
            title: "Real-Time Ray Tracing Shader Engine in Vulkan & C++",
            description: "Build a high-performance 3D graphics rendering pipeline featuring dynamic ray tracing, PBR materials, and custom shader compute passes.",
            providerId: SeedConstants.Providers.ApexGamesId,
            majorId: GetMajorId("Game Development & Interactive Media"),
            specialtyId: GetSpecialtyId("Game Development & Interactive Media", "3D Game Engine Architecture"),
            createdAt: DateTime.UtcNow.AddDays(-6)
        );

        AttachSkills(gameTemplate, "3D Computer Graphics & Shader Programming", "Unreal Engine & C++", "C++ Low-Level Development");

        // Milestone 1 (Single task)
        gameTemplate.AddMilestone("Vulkan Swapchain & Context Setup", "Initialize raw Vulkan device instances and swapchain image buffers.", 20.0m, 25.00m, 20.00m);
        var gM1 = gameTemplate.GlobalMilestones.First(m => m.Title == "Vulkan Swapchain & Context Setup");
        gameTemplate.AddGlobalTaskToMilestone(gM1.Id, "Vulkan Boilerplate Context Module", "Construct physical device queue and swapchain setup.", 100.00m, DeliverableType.Url);

        // Milestone 2 (Single task)
        gameTemplate.AddMilestone("Ray Tracing Pipeline & Shader Modules", "Write SPIR-V ray generation, closest hit, and miss shaders.", 45.0m, 45.00m, 50.00m);
        var gM2 = gameTemplate.GlobalMilestones.First(m => m.Title == "Ray Tracing Pipeline & Shader Modules");
        gameTemplate.AddGlobalTaskToMilestone(gM2.Id, "GLSL SPIR-V Ray Tracing Shaders", "Implement BHDR path tracing calculation shaders.", 100.00m, DeliverableType.File);

        // Milestone 3 (Single task)
        gameTemplate.AddMilestone("Performance Benchmark & Demo Release", "Profile FPS frame times across varying triangle light counts.", 25.0m, 30.00m, 30.00m);
        var gM3 = gameTemplate.GlobalMilestones.First(m => m.Title == "Performance Benchmark & Demo Release");
        gameTemplate.AddGlobalTaskToMilestone(gM3.Id, "Executable Binary & Benchmarks Upload", "Upload standalone demo installer and frame time graphs.", 100.00m, DeliverableType.File);

        gameTemplate.AddMilestoneDependency(gM2.Id, gM1.Id, DependencyType.FinishToStart);
        gameTemplate.AddMilestoneDependency(gM3.Id, gM2.Id, DependencyType.FinishToStart);

        gameTemplate.SubmitForReview();
        gameTemplate.Approve();
        await context.ProjectTemplates.AddAsync(gameTemplate);

        // =========================================================================
        // --- Template 6: Genomic Data Pipeline Processor ---
        // Status: PendingReview
        // Topology: Deep Sequential Chain (5 Levels deep, 10 Tasks)
        // Purpose: Tests Admin Reviewer Approval Queue UI
        // Track: Bioinformatics & Health Informatics
        // =========================================================================
        var bioTemplate = new ProjectTemplate(
            title: "Scalable Genomic Sequence Alignment & Variant Calling Engine",
            description: "Construct a high-throughput genomic data processing pipeline capable of aligning FASTQ reads and detecting genetic variants adhering to FHIR clinical standards.",
            providerId: SeedConstants.Providers.CloudSystemsId,
            majorId: GetMajorId("Bioinformatics & Health Informatics"),
            specialtyId: GetSpecialtyId("Bioinformatics & Health Informatics", "Medical Data Analytics & Computational Genomics"),
            createdAt: DateTime.UtcNow.AddDays(-1)
        );

        AttachSkills(bioTemplate, "Genomic Data Analysis", "HL7 / FHIR Health Data Standards", "Python Machine Learning", "Data Engineering & Apache Spark");

        bioTemplate.AddMilestone("FASTQ Quality Control & Preprocessing", "Filter low-quality DNA sequencing reads.", 15.0m, 15.00m, 15.00m);
        var bM1 = bioTemplate.GlobalMilestones.First(m => m.Title == "FASTQ Quality Control & Preprocessing");
        bioTemplate.AddGlobalTaskToMilestone(bM1.Id, "Adapter Trimming Script", "Script automated adapter sequence removal.", 50.00m, DeliverableType.File);
        bioTemplate.AddGlobalTaskToMilestone(bM1.Id, "Phred Score Filter", "Build quality score statistical charts.", 50.00m, DeliverableType.Text);

        bioTemplate.AddMilestone("Reference Genome Indexing", "Build Burrows-Wheeler transform indexes.", 20.0m, 20.00m, 20.00m);
        var bM2 = bioTemplate.GlobalMilestones.First(m => m.Title == "Reference Genome Indexing");
        bioTemplate.AddGlobalTaskToMilestone(bM2.Id, "BWT Indexing Module", "Index human reference genome HG38.", 50.00m, DeliverableType.Url);
        bioTemplate.AddGlobalTaskToMilestone(bM2.Id, "Memory Usage Profiling", "Optimize RAM consumption for 3GB genome lookup.", 50.00m, DeliverableType.File);

        bioTemplate.AddMilestone("Sequence Alignment & BAM File Generation", "Align reads against reference genome.", 25.0m, 25.00m, 25.00m);
        var bM3 = bioTemplate.GlobalMilestones.First(m => m.Title == "Sequence Alignment & BAM File Generation");
        bioTemplate.AddGlobalTaskToMilestone(bM3.Id, "SAM/BAM Alignment Pipeline", "Execute parallel alignment threads.", 50.00m, DeliverableType.Url);
        bioTemplate.AddGlobalTaskToMilestone(bM3.Id, "Duplicate Read Marking", "Remove PCR duplicate artifacts.", 50.00m, DeliverableType.Text);

        bioTemplate.AddMilestone("Variant Calling & VCF Formatting", "Identify single nucleotide polymorphisms (SNPs).", 20.0m, 20.00m, 20.00m);
        var bM4 = bioTemplate.GlobalMilestones.First(m => m.Title == "Variant Calling & VCF Formatting");
        bioTemplate.AddGlobalTaskToMilestone(bM4.Id, "Haplotype Variant Detector", "Call genetic variants using Bayesian model.", 50.00m, DeliverableType.File);
        bioTemplate.AddGlobalTaskToMilestone(bM4.Id, "VCF Standard Validator", "Validate VCF file structure.", 50.00m, DeliverableType.Text);

        bioTemplate.AddMilestone("FHIR Clinical Data Integration", "Export genetic variant calls into HL7/FHIR compliant JSON payloads.", 20.0m, 20.00m, 20.00m);
        var bM5 = bioTemplate.GlobalMilestones.First(m => m.Title == "FHIR Clinical Data Integration");
        bioTemplate.AddGlobalTaskToMilestone(bM5.Id, "FHIR Molecular Sequence Resource Mapping", "Transform VCF rows into FHIR resources.", 50.00m, DeliverableType.Url);
        bioTemplate.AddGlobalTaskToMilestone(bM5.Id, "Final Thesis Report", "Submit complete clinical validation documentation.", 50.00m, DeliverableType.File);

        // Deep Sequential Dependencies
        bioTemplate.AddMilestoneDependency(bM2.Id, bM1.Id, DependencyType.FinishToStart);
        bioTemplate.AddMilestoneDependency(bM3.Id, bM2.Id, DependencyType.FinishToStart);
        bioTemplate.AddMilestoneDependency(bM4.Id, bM3.Id, DependencyType.FinishToStart);
        bioTemplate.AddMilestoneDependency(bM5.Id, bM4.Id, DependencyType.FinishToStart);

        bioTemplate.SubmitForReview(); // Leaves in PendingReview state
        await context.ProjectTemplates.AddAsync(bioTemplate);

        // =========================================================================
        // --- Template 7: AI Medical Image Diagnostics ---
        // Status: PendingReview (Submitted for changes testing)
        // Topology: Asymmetric Weighting (M1: 10% WBS / 40% Grade, M2: 70% WBS / 40% Grade)
        // Track: Artificial Intelligence & Data Science
        // =========================================================================
        var aiMedicalTemplate = new ProjectTemplate(
            title: "Deep Learning Medical Image Segmentation for Oncology Diagnostics",
            description: "Train 3D U-Net convolutional neural networks to automatically segment tumor boundaries in MRI/CT DICOM scans.",
            providerId: SeedConstants.Providers.AcmeCorpId,
            majorId: GetMajorId("Artificial Intelligence & Data Science"),
            specialtyId: GetSpecialtyId("Artificial Intelligence & Data Science", "Machine Learning & Deep Learning"),
            createdAt: DateTime.UtcNow.AddDays(-4)
        );

        AttachSkills(aiMedicalTemplate, "PyTorch & Deep Learning", "Computer Vision & OpenCV", "Python Machine Learning");

        // Asymmetric Weights: Low effort WBS (10%), High Grade Impact (40%)
        aiMedicalTemplate.AddMilestone("Clinical Literature & Ethical AI Formulation", "Survey medical imaging ethics, privacy guidelines, and network architectures.", 10.0m, 10.00m, 40.00m);
        var aiM1 = aiMedicalTemplate.GlobalMilestones.First(m => m.Title == "Clinical Literature & Ethical AI Formulation");
        aiMedicalTemplate.AddGlobalTaskToMilestone(aiM1.Id, "HIPAA & Privacy Protocol Compliance", "Document de-identification bounds.", 50.00m, DeliverableType.Text);
        aiMedicalTemplate.AddGlobalTaskToMilestone(aiM1.Id, "U-Net Architecture Survey Paper", "Compile paper comparing CNN vs. Vision Transformers.", 50.00m, DeliverableType.File);

        // Asymmetric Weights: Massive Effort WBS (70%), Equal Grade Impact (40%)
        aiMedicalTemplate.AddMilestone("Model Training & Hyperparameter Tuning", "Train 3D U-Net across 50,000 DICOM scans on GPU clusters.", 80.0m, 70.00m, 40.00m);
        var aiM2 = aiMedicalTemplate.GlobalMilestones.First(m => m.Title == "Model Training & Hyperparameter Tuning");
        aiMedicalTemplate.AddGlobalTaskToMilestone(aiM2.Id, "PyTorch Model Pipeline", "Construct data loader and loss function scripts.", 50.00m, DeliverableType.Url);
        aiMedicalTemplate.AddGlobalTaskToMilestone(aiM2.Id, "Dice Coefficient Loss Benchmarks", "Log model validation metrics.", 50.00m, DeliverableType.File);

        aiMedicalTemplate.AddMilestone("Clinical Evaluation & Thesis Defense", "Evaluate inference precision against expert radiologist annotations.", 20.0m, 20.00m, 20.00m);
        var aiM3 = aiMedicalTemplate.GlobalMilestones.First(m => m.Title == "Clinical Evaluation & Thesis Defense");
        aiMedicalTemplate.AddGlobalTaskToMilestone(aiM3.Id, "Radiologist Agreement Comparison", "Compute confusion matrix metrics.", 50.00m, DeliverableType.Text);
        aiMedicalTemplate.AddGlobalTaskToMilestone(aiM3.Id, "Final Oncology Capstone Thesis", "Upload finalized defense document.", 50.00m, DeliverableType.File);

        aiMedicalTemplate.AddMilestoneDependency(aiM2.Id, aiM1.Id, DependencyType.FinishToStart);
        aiMedicalTemplate.AddMilestoneDependency(aiM3.Id, aiM2.Id, DependencyType.FinishToStart);

        aiMedicalTemplate.SubmitForReview();
        await context.ProjectTemplates.AddAsync(aiMedicalTemplate);

        // =========================================================================
        // --- Template 8: Experimental Quantum Simulator ---
        // Status: Draft (Not submitted)
        // Topology: Empty & Boundary Edge Cases (M1 has 0 tasks)
        // Track: Theoretical Quantum Computing Studies
        // =========================================================================
        var quantumTemplate = new ProjectTemplate(
            title: "Fault-Tolerant Quantum Circuit Simulator in Qiskit & C++",
            description: "Construct a multi-qubit statevector simulator evaluating quantum error correction codes and quantum key distribution protocols.",
            providerId: SeedConstants.Providers.CloudSystemsId,
            majorId: GetMajorId("Theoretical Quantum Computing Studies"),
            specialtyId: null, // Boundary Test: Major with 0 specialties
            createdAt: DateTime.UtcNow
        );

        AttachSkills(quantumTemplate, "Quantum Computing & Qiskit", "C++ Low-Level Development");

        // Milestone 1 (Boundary Case: 0 Tasks inside milestone container)
        quantumTemplate.AddMilestone("Quantum Gates Linear Algebra Proofs", "Derive state matrix transformations for Hadamard and CNOT gates.", 15.0m, 50.00m, 50.00m);

        // Milestone 2 (Standard 1 Task)
        quantumTemplate.AddMilestone("Qiskit Execution Harness", "Connect statevector engine to IBM Quantum simulator endpoints.", 20.0m, 50.00m, 50.00m);
        var qM2 = quantumTemplate.GlobalMilestones.First(m => m.Title == "Qiskit Execution Harness");
        quantumTemplate.AddGlobalTaskToMilestone(qM2.Id, "Qiskit API Client C++ Binding", "Construct SWIG interface bindings.", 100.00m, DeliverableType.Url);

        // Saved as Draft (No SubmitForReview or Approve called)
        await context.ProjectTemplates.AddAsync(quantumTemplate);

        // =========================================================================
        // --- Template 9: Autonomous Swarm Safety Framework ---
        // Status: ChangesRequested (3)
        // Feature: Reviewer requested modifications before approval
        // Track: Computer Engineering / IoT & Embedded Systems
        // =========================================================================
        var changesRequestedTemplate = new ProjectTemplate(
            title: "Hardware-In-The-Loop Safety Override Framework for Autonomous Swarms",
            description: "Develop a fail-safe physical signal interrupt mechanism and emergency software kill-switch protocol for edge autonomous swarms.",
            providerId: SeedConstants.Providers.AcmeCorpId,
            majorId: GetMajorId("Computer Engineering"),
            specialtyId: GetSpecialtyId("Computer Engineering", "IoT & Embedded Systems"),
            createdAt: DateTime.UtcNow.AddDays(-7)
        );

        AttachSkills(changesRequestedTemplate, "C Systems Programming", "Microcontrollers & Embedded C", "Network Security & Threat Modeling");

        changesRequestedTemplate.AddMilestone("Hardware Interrupt Signal Circuit", "Design GPIO emergency circuit interrupt routing.", 20.0m, 50.00m, 50.00m);
        var crM1 = changesRequestedTemplate.GlobalMilestones.First(m => m.Title == "Hardware Interrupt Signal Circuit");
        changesRequestedTemplate.AddGlobalTaskToMilestone(crM1.Id, "GPIO Pin Schematic", "Draft schematic for physical interrupt line.", 100.00m, DeliverableType.File);

        changesRequestedTemplate.AddMilestone("Remote Telemetry Kill-Switch Software", "Build wireless UDP keep-alive watchdog daemons.", 25.0m, 50.00m, 50.00m);
        var crM2 = changesRequestedTemplate.GlobalMilestones.First(m => m.Title == "Remote Telemetry Kill-Switch Software");
        changesRequestedTemplate.AddGlobalTaskToMilestone(crM2.Id, "Watchdog Daemon C Code", "Write hardware timer reset loop.", 100.00m, DeliverableType.Url);

        changesRequestedTemplate.AddMilestoneDependency(crM2.Id, crM1.Id, DependencyType.FinishToStart);

        // Transition: Draft -> PendingReview -> ChangesRequested
        changesRequestedTemplate.SubmitForReview();
        changesRequestedTemplate.RequestChanges("Please add an explicit safety verification milestone and expand task deliverables for manual override testing.");

        await context.ProjectTemplates.AddAsync(changesRequestedTemplate);

        // =========================================================================
        // --- Template 10: Enterprise ERP Integration Platform ---
        // Status: PendingProviderAcceptance (4)
        // Feature: Reviewer edited template directly; pending provider confirmation
        // Track: Information Systems & Digital Business
        // =========================================================================
        var pendingAcceptanceTemplate = new ProjectTemplate(
            title: "Enterprise ERP Subsystem Integration & Data Bus Connector",
            description: "Implement an asynchronous event-driven data bus connecting legacy SAP/Oracle ERP modules using modern messaging queues.",
            providerId: SeedConstants.Providers.CloudSystemsId,
            majorId: GetMajorId("Information Systems & Digital Business"),
            specialtyId: GetSpecialtyId("Information Systems & Digital Business", "Enterprise Resource Planning (ERP)"),
            createdAt: DateTime.UtcNow.AddDays(-5)
        );

        AttachSkills(pendingAcceptanceTemplate, "Microservices Architecture", "SQL & Relational Modeling", "Business Process Modeling");

        pendingAcceptanceTemplate.AddMilestone("Message Bus Schema Design", "Formulate Apache Kafka event schema standards for ERP transactions.", 15.0m, 40.00m, 40.00m);
        var paM1 = pendingAcceptanceTemplate.GlobalMilestones.First(m => m.Title == "Message Bus Schema Design");
        pendingAcceptanceTemplate.AddGlobalTaskToMilestone(paM1.Id, "Avro Event Specification", "Define Protobuf/Avro transactional events.", 100.00m, DeliverableType.File);

        pendingAcceptanceTemplate.AddMilestone("Connector Service & Transactional Tests", "Construct idempotent API ingestion workers.", 30.0m, 60.00m, 60.00m);
        var paM2 = pendingAcceptanceTemplate.GlobalMilestones.First(m => m.Title == "Connector Service & Transactional Tests");
        pendingAcceptanceTemplate.AddGlobalTaskToMilestone(paM2.Id, "Ingestion Worker Code", "Publish benchmarked ingestion service.", 100.00m, DeliverableType.Url);

        pendingAcceptanceTemplate.AddMilestoneDependency(paM2.Id, paM1.Id, DependencyType.FinishToStart);

        // Transition: Draft -> PendingReview -> PendingProviderAcceptance
        pendingAcceptanceTemplate.SubmitForReview();

        // Corrected: Passing both required parameters (adjustedTitle, adjustedDescription)
        pendingAcceptanceTemplate.ProposeReviewerChanges(
            adjustedTitle: pendingAcceptanceTemplate.Title,
            adjustedDescription: "Implement an asynchronous event-driven data bus connecting legacy SAP/Oracle ERP modules using modern messaging queues. (Re-adjusted Milestone 1 and 2 grading weights to 40/60 to better reflect implementation scope)."
        );

        await context.ProjectTemplates.AddAsync(pendingAcceptanceTemplate);

        // =========================================================================
        // --- Template 11: Automated Cryptocurrency Arbitrage Bot ---
        // Status: Rejected (6)
        // Feature: Permanently rejected policy violation edge case
        // Track: Software Engineering
        // =========================================================================
        var rejectedTemplate = new ProjectTemplate(
            title: "High-Frequency Cryptocurrency Flash-Loan Arbitrage Bot Engine",
            description: "Build an automated high-frequency trading bot executing flash loans and mempool front-running across decentralized exchanges.",
            providerId: SeedConstants.Providers.AcmeCorpId,
            majorId: GetMajorId("Software Engineering"),
            specialtyId: GetSpecialtyId("Software Engineering", "Full-Stack Web Systems"),
            createdAt: DateTime.UtcNow.AddDays(-12)
        );

        AttachSkills(rejectedTemplate, "C# .NET Backend Development", "Cryptography & Security Protocols");

        rejectedTemplate.AddMilestone("Mempool Scanner & Smart Contract Flash Loan", "Script automated mempool parsing daemons.", 20.0m, 100.00m, 100.00m);
        var rejM1 = rejectedTemplate.GlobalMilestones.First(m => m.Title == "Mempool Scanner & Smart Contract Flash Loan");
        rejectedTemplate.AddGlobalTaskToMilestone(rejM1.Id, "Mempool Parser Module", "Script Web3 WebSocket transaction listeners.", 100.00m, DeliverableType.Url);

        // Transition: Draft -> PendingReview -> Rejected
        rejectedTemplate.SubmitForReview();
        rejectedTemplate.RejectPermanently("Proposed project topic involves high-frequency cryptocurrency front-running and financial trading bots, violating platform academic policy guidelines.");

        await context.ProjectTemplates.AddAsync(rejectedTemplate);

        // =========================================================================
        // --- Template 12: Legacy Monolithic ASP.NET WebForms Migration ---
        // Status: Archived (7)
        // Feature: Previously approved project template now retired from public search
        // Track: Software Engineering
        // =========================================================================
        var archivedTemplate = new ProjectTemplate(
            title: "Legacy ASP.NET WebForms to .NET Core Monolith Refactoring",
            description: "Deconstruct legacy WebForms application architectures and migrate SOAP services to RESTful Web APIs.",
            providerId: SeedConstants.Providers.CloudSystemsId,
            majorId: GetMajorId("Software Engineering"),
            specialtyId: GetSpecialtyId("Software Engineering", "Full-Stack Web Systems"),
            createdAt: DateTime.UtcNow.AddDays(-180)
        );

        AttachSkills(archivedTemplate, "C# .NET Backend Development", "ASP.NET Core Web API", "SQL & Relational Modeling");

        archivedTemplate.AddMilestone("SOAP Service Mapping", "Document legacy WSDL SOAP endpoints.", 15.0m, 40.00m, 40.00m);
        var arcM1 = archivedTemplate.GlobalMilestones.First(m => m.Title == "SOAP Service Mapping");
        archivedTemplate.AddGlobalTaskToMilestone(arcM1.Id, "WSDL Endpoint Audit Log", "Catalog all XML payload structures.", 100.00m, DeliverableType.File);

        archivedTemplate.AddMilestone("REST API Replacement Construction", "Rebuild legacy SOAP handlers as ASP.NET Core controllers.", 30.0m, 60.00m, 60.00m);
        var arcM2 = archivedTemplate.GlobalMilestones.First(m => m.Title == "REST API Replacement Construction");
        archivedTemplate.AddGlobalTaskToMilestone(arcM2.Id, "ASP.NET Core Web API Service", "Publish modern REST replacement controllers.", 100.00m, DeliverableType.Url);

        archivedTemplate.AddMilestoneDependency(arcM2.Id, arcM1.Id, DependencyType.FinishToStart);

        // Transition: Draft -> PendingReview -> Approved -> Archived
        archivedTemplate.SubmitForReview();
        archivedTemplate.Approve();

        // Explicitly override status property for Archived state in EF Core tracker
        context.Entry(archivedTemplate).Property(t => t.Status).CurrentValue = AcademicGateway.Domain.ProjectTemplates.Enums.ProjectTemplateStatus.Archived;

        await context.ProjectTemplates.AddAsync(archivedTemplate);

        await context.SaveChangesAsync();
    }
}
using AcademicGateway.Domain.ProjectInstances;
using AcademicGateway.Domain.ProjectInstances.Enums;
using AcademicGateway.Domain.ProjectInstances.Grading;
using AcademicGateway.Domain.ProjectInstances.Services;
using Infrastructure.Persistence.Context.Seeders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AcademicGateway.Infrastructure.Persistence.Context.Seeders;

/// <summary>
/// Seeder responsible for initializing live running project workspaces, supervision requests,
/// task deliverable submissions, professor evaluations, milestone comments, and tech support mentorship proposals.
/// Seeds 6 workspace scenarios covering all runtime project lifecycle states and workflow edge cases.
/// </summary>
public static class ProjectInstanceSeeder
{
    /// <summary>
    /// Evaluates and seeds baseline project instances and runtime workflow artifacts.
    /// </summary>
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.ProjectInstances.AnyAsync())
        {
            return;
        }

        // 1. Retrieve prerequisite approved project templates with nested WBS data and inbound dependencies
        var templates = await context.ProjectTemplates
            .Include(t => t.ProjectTemplateSkills)
            .Include(t => t.GlobalMilestones)
                .ThenInclude(m => m.GlobalTasks)
            .Include(t => t.GlobalMilestones)
                .ThenInclude(m => m.InboundDependencies)
            .ToListAsync();

        if (!templates.Any())
        {
            return;
        }

        var milestoneFactory = new LocalMilestoneFactory();
        var gradingStrategy = new PercentageGradingStrategy();
        var now = DateTime.UtcNow;

        // Template lookups by title
        var cloudTemplate = templates.FirstOrDefault(t => t.Title == "Distributed E-Commerce Cloud Infrastructure Migration");
        var infraTemplate = templates.FirstOrDefault(t => t.Title == "Enterprise Multi-Region Infrastructure Layer Orchestration");
        var securityTemplate = templates.FirstOrDefault(t => t.Title == "Zero-Trust Container Security & Penetration Testing Suite");
        var gameTemplate = templates.FirstOrDefault(t => t.Title == "Real-Time Ray Tracing Shader Engine in Vulkan & C++");
        var roboticsTemplate = templates.FirstOrDefault(t => t.Title == "Multi-Agent Swarm Robotics & Spatial Perception Engine");

        // =========================================================================
        // --- Scenario 1: E-Commerce Cloud Migration ---
        // Primary Actors: Student (Jane Doe), Professor (Dr. Alan Turing)
        // Instance Status: AwaitingSupervision
        // Workflow State: Supervision request created and pending in professor queue
        // =========================================================================
        if (cloudTemplate != null)
        {
            var project1 = cloudTemplate.Instantiate(
                studentId: SeedConstants.Students.JaneDoeId,
                createdAt: now.AddDays(-5),
                milestoneFactory: milestoneFactory,
                initialRequestedProfessorId: SeedConstants.Professors.AlanTuringId
            );

            await context.ProjectInstances.AddAsync(project1);
        }

        // =========================================================================
        // --- Scenario 2: Multi-Region Infra Orchestration ---
        // Primary Actors: Student (Sophia Chen), Professor (Dr. Alan Turing), Mentor (Alan Vance)
        // Instance Status: Active
        // Workflow State: Active project with completed M1 deliverables, grades, Q&A comments, and accepted Tech Support
        // =========================================================================
        if (infraTemplate != null)
        {
            var project2 = infraTemplate.Instantiate(
                studentId: SeedConstants.Students.SophiaChenId,
                createdAt: now.AddDays(-30),
                milestoneFactory: milestoneFactory,
                initialRequestedProfessorId: SeedConstants.Professors.AlanTuringId
            );

            // 1. Accept Supervision Request
            var supervisionReq = project2.SupervisionRequests.First();
            project2.ReviewSupervisionRequest(
                requestId: supervisionReq.Id,
                accept: true,
                rejectionReason: null,
                reviewedAt: now.AddDays(-28)
            );

            var profTuring = await context.Professors.FindAsync(SeedConstants.Professors.AlanTuringId);
            profTuring?.IncrementActiveProjects();

            // 2. Submit and Evaluate Milestone 1 Tasks (Corrected to TitleSnapshot)
            var m1 = project2.LocalMilestones.First(m => m.TitleSnapshot == "Infrastructure Base Network & Security Provisioning");
            var vpcTask = m1.LocalTasks.First(t => t.TitleSnapshot == "VPC & Subnet Topologies Setup");
            var iamTask = m1.LocalTasks.First(t => t.TitleSnapshot == "IAM Access Control Rules");

            project2.SubmitTaskDeliverable(
                milestoneId: m1.Id,
                taskId: vpcTask.Id,
                submissionPayload: "https://github.com/sophiachen/multi-region-infra/tree/main/terraform/vpc",
                utcNow: now.AddDays(-20)
            );

            project2.EvaluateTaskSubmission(
                milestoneId: m1.Id,
                taskId: vpcTask.Id,
                grade: 95.00m,
                feedback: "Excellent multi-region VPC CIDR subnet partitioning. High availability bounds verified.",
                gradingStrategy: gradingStrategy,
                utcNow: now.AddDays(-18),
                executingUserId: SeedConstants.Professors.AlanTuringId
            );

            project2.SubmitTaskDeliverable(
                milestoneId: m1.Id,
                taskId: iamTask.Id,
                submissionPayload: "https://storage.academicgateway.internal/deliverables/sophia-iam-matrix.pdf",
                utcNow: now.AddDays(-15)
            );

            project2.EvaluateTaskSubmission(
                milestoneId: m1.Id,
                taskId: iamTask.Id,
                grade: 92.00m,
                feedback: "Solid principle of least privilege IAM policy definitions.",
                gradingStrategy: gradingStrategy,
                utcNow: now.AddDays(-14),
                executingUserId: SeedConstants.Professors.AlanTuringId
            );

            // 3. Add Milestone Discussion Comments
            project2.AddMilestoneComment(
                milestoneId: m1.Id,
                authorId: SeedConstants.Students.SophiaChenId,
                authorIdentitySnapshot: "Sophia Chen (Student)",
                content: "Dr. Turing, I have attached the Terraform plan outputs for the secondary region failover routes.",
                utcNow: now.AddDays(-19)
            );

            project2.AddMilestoneComment(
                milestoneId: m1.Id,
                authorId: SeedConstants.Professors.AlanTuringId,
                authorIdentitySnapshot: "Dr. Alan Turing (Supervisor)",
                content: "Looks comprehensive, Sophia. Ensure you verify cross-region latency limits during the M4 load tests.",
                utcNow: now.AddDays(-18)
            );

            // 4. Corporate Tech Support Mentorship Proposal & Acceptance
            project2.ProposeTechSupport(
                techSupportAccountId: SeedConstants.TechSupport.AlanVanceId,
                utcNow: now.AddDays(-12)
            );

            var techProposal = project2.TechSupportProposals.First();
            project2.ReviewTechSupportProposal(
                proposalId: techProposal.Id,
                accept: true,
                rejectionReason: null
            );

            await context.ProjectInstances.AddAsync(project2);
        }

        // =========================================================================
        // --- Scenario 3: Zero-Trust Cybersecurity Penetration Lab ---
        // Primary Actors: Student (Bob Williams), Prof 1 (Dr. Hinton - Rejected), Prof 2 (Dr. Ada Lovelace - Accepted)
        // Instance Status: Active
        // Workflow State: Re-requested supervision workflow; M1 task submitted waiting for evaluation
        // =========================================================================
        if (securityTemplate != null)
        {
            var project3 = securityTemplate.Instantiate(
                studentId: SeedConstants.Students.BobWilliamsId,
                createdAt: now.AddDays(-14),
                milestoneFactory: milestoneFactory,
                initialRequestedProfessorId: SeedConstants.Professors.GeoffreyHintonId
            );

            // 1. Initial Professor Rejection (Capacity Limit Edge Case)
            var initialReq = project3.SupervisionRequests.First();
            project3.ReviewSupervisionRequest(
                requestId: initialReq.Id,
                accept: false,
                rejectionReason: "Supervision capacity currently reached for AI and security tracks this term.",
                reviewedAt: now.AddDays(-13)
            );

            // 2. Secondary Supervision Request to Dr. Ada Lovelace
            project3.SubmitSupervisionRequest(
                professorId: SeedConstants.Professors.AdaLovelaceId,
                pitchText: "Seeking academic supervision for zero-trust microservice perimeter penetration lab.",
                utcNow: now.AddDays(-12)
            );

            var secondaryReq = project3.SupervisionRequests.First(r => r.ProfessorId == SeedConstants.Professors.AdaLovelaceId);
            project3.ReviewSupervisionRequest(
                requestId: secondaryReq.Id,
                accept: true,
                rejectionReason: null,
                reviewedAt: now.AddDays(-11)
            );

            var profLovelace = await context.Professors.FindAsync(SeedConstants.Professors.AdaLovelaceId);
            profLovelace?.IncrementActiveProjects();

            // 3. Deliverable Submitted (Waiting in Professor Review Queue - Corrected to TitleSnapshot)
            var m1 = project3.LocalMilestones.First(m => m.TitleSnapshot == "Threat Modeling & Boundary Definition");
            var strideTask = m1.LocalTasks.First(t => t.TitleSnapshot == "STRIDE Threat Matrix");

            project3.SubmitTaskDeliverable(
                milestoneId: m1.Id,
                taskId: strideTask.Id,
                submissionPayload: "https://storage.academicgateway.internal/deliverables/bob-stride-analysis.pdf",
                utcNow: now.AddDays(-2)
            );

            await context.ProjectInstances.AddAsync(project3);
        }

        // =========================================================================
        // --- Scenario 4: Real-Time Vulkan Shader Engine ---
        // Primary Actors: Student (Eva Martinez), Professor (Dr. John von Neumann)
        // Instance Status: Concluded
        // Workflow State: 100% completed project; all single-task milestones evaluated with high marks
        // =========================================================================
        if (gameTemplate != null)
        {
            var project4 = gameTemplate.Instantiate(
                studentId: SeedConstants.Students.EvaMartinezId,
                createdAt: now.AddDays(-90),
                milestoneFactory: milestoneFactory,
                initialRequestedProfessorId: SeedConstants.Professors.JohnVonNeumannId
            );

            var supervisionReq = project4.SupervisionRequests.First();
            project4.ReviewSupervisionRequest(
                requestId: supervisionReq.Id,
                accept: true,
                rejectionReason: null,
                reviewedAt: now.AddDays(-88)
            );

            var profVonNeumann = await context.Professors.FindAsync(SeedConstants.Professors.JohnVonNeumannId);
            profVonNeumann?.IncrementActiveProjects();

            // Complete M1
            var gM1 = project4.LocalMilestones.First(m => m.TitleSnapshot == "Vulkan Swapchain & Context Setup");
            var gTask1 = gM1.LocalTasks.First();
            project4.SubmitTaskDeliverable(gM1.Id, gTask1.Id, "https://github.com/evamartinez/vulkan-rt/tree/main/context", now.AddDays(-70));
            project4.EvaluateTaskSubmission(gM1.Id, gTask1.Id, 98.00m, "Flawless physical device queue initialization.", gradingStrategy, now.AddDays(-68), SeedConstants.Professors.JohnVonNeumannId);

            // Complete M2
            var gM2 = project4.LocalMilestones.First(m => m.TitleSnapshot == "Ray Tracing Pipeline & Shader Modules");
            var gTask2 = gM2.LocalTasks.First();
            project4.SubmitTaskDeliverable(gM2.Id, gTask2.Id, "https://storage.academicgateway.internal/deliverables/eva-spirv-shaders.zip", now.AddDays(-40));
            project4.EvaluateTaskSubmission(gM2.Id, gTask2.Id, 96.00m, "PATH tracing GLSL shaders compile cleanly with optimal BVH traversal.", gradingStrategy, now.AddDays(-38), SeedConstants.Professors.JohnVonNeumannId);

            // Complete M3
            var gM3 = project4.LocalMilestones.First(m => m.TitleSnapshot == "Performance Benchmark & Demo Release");
            var gTask3 = gM3.LocalTasks.First();
            project4.SubmitTaskDeliverable(gM3.Id, gTask3.Id, "https://storage.academicgateway.internal/deliverables/eva-vulkan-demo-setup.exe", now.AddDays(-10));
            project4.EvaluateTaskSubmission(gM3.Id, gTask3.Id, 100.00m, "Outstanding 60 FPS performance benchmark at 4K resolution.", gradingStrategy, now.AddDays(-5), SeedConstants.Professors.JohnVonNeumannId);

            // Transition state to Concluded and finalize grade
            project4.ConcludeProject();
            project4.FinalizeProjectGrade(gradingStrategy, now.AddDays(-4), SeedConstants.Professors.JohnVonNeumannId);

            await context.ProjectInstances.AddAsync(project4);
        }

        // =========================================================================
        // --- Scenario 5: Autonomous Swarm Robotics ---
        // Primary Actors: Student (Alexander Hohenzollern), Professor (Dr. Claude Shannon)
        // Instance Status: Active
        // Workflow State: Complex DAG track execution (Track A PCB complete, Track B Perception active)
        // =========================================================================
        if (roboticsTemplate != null)
        {
            var project5 = roboticsTemplate.Instantiate(
                studentId: SeedConstants.Students.AlexanderHohenzollernId,
                createdAt: now.AddDays(-25),
                milestoneFactory: milestoneFactory,
                initialRequestedProfessorId: SeedConstants.Professors.ClaudeShannonId
            );

            var supervisionReq = project5.SupervisionRequests.First();
            project5.ReviewSupervisionRequest(
                requestId: supervisionReq.Id,
                accept: true,
                rejectionReason: null,
                reviewedAt: now.AddDays(-24)
            );

            var profShannon = await context.Professors.FindAsync(SeedConstants.Professors.ClaudeShannonId);
            profShannon?.IncrementActiveProjects();

            // Track A: Complete Hardware PCB Milestone Tasks
            var hM1 = project5.LocalMilestones.First(m => m.TitleSnapshot == "Hardware PCB & Micro-Sensor Fabrication");
            foreach (var task in hM1.LocalTasks)
            {
                project5.SubmitTaskDeliverable(hM1.Id, task.Id, $"https://storage.academicgateway.internal/deliverables/alexander-{task.Id}.pdf", now.AddDays(-15));
                project5.EvaluateTaskSubmission(hM1.Id, task.Id, 91.00m, "Hardware schematic and pin calibration verified.", gradingStrategy, now.AddDays(-12), SeedConstants.Professors.ClaudeShannonId);
            }

            await context.ProjectInstances.AddAsync(project5);
        }

        // =========================================================================
        // --- Scenario 6: Abandoned / Cancelled Project ---
        // Primary Actors: Student (Charlie Brown), Professor (Dr. Alan Turing)
        // Instance Status: Canceled
        // Workflow State: Student initiated workspace but cancelled project early
        // =========================================================================
        if (cloudTemplate != null)
        {
            var project6 = cloudTemplate.Instantiate(
                studentId: SeedConstants.Students.CharlieBrownId,
                createdAt: now.AddDays(-40),
                milestoneFactory: milestoneFactory,
                initialRequestedProfessorId: SeedConstants.Professors.AlanTuringId
            );

            // Execute cancellation domain workflow
            project6.CancelProject();

            await context.ProjectInstances.AddAsync(project6);
        }

        await context.SaveChangesAsync();
    }
}
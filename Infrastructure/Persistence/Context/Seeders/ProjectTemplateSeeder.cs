using AcademicGateway.Domain.Common.Enums;
using AcademicGateway.Domain.ProjectInstances.Services;
using AcademicGateway.Domain.ProjectTemplates;
using AcademicGateway.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AcademicGateway.Infrastructure.Persistence.Context.Seeders;

/// <summary>
/// Seeder responsible for populating baseline project templates and initializing sample live workspace instances.
/// </summary>
public static class ProjectTemplateSeeder
{
    /// <summary>
    /// Evaluates and seeds baseline project templates, global milestones, tasks, and initial live project instances.
    /// </summary>
    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        if (await context.ProjectTemplates.AnyAsync())
        {
            return;
        }

        // Look up prerequisite seeded identity accounts [source: 2]
        var defaultProviderUser = await userManager.FindByEmailAsync("partner@acmesolutions.internal");
        var defaultProfessorUser = await userManager.FindByEmailAsync("professor@academicgateway.com");
        var defaultStudentUser = await userManager.FindByEmailAsync("student@academicgateway.com");

        if (defaultProviderUser == null || defaultProfessorUser == null || defaultStudentUser == null)
        {
            return;
        }

        // Retrieve baseline lookup data [source: 2]
        var seededSkills = await context.Skills.ToListAsync();

        var computerScienceMajor = await context.Majors
            .Include(m => m.Specialties)
            .FirstOrDefaultAsync(m => m.Name == "Computer Science");

        Guid? sampleMajorId = computerScienceMajor?.Id;
        Guid? sampleSpecialtyId = computerScienceMajor?.Specialties
            .FirstOrDefault(s => s.Name == "Software Engineering")?.Id;

        // =========================================================================
        // --- Template 1: Cloud Migration --- [source: 2]
        // =========================================================================
        var cloudTemplate = new ProjectTemplate(
            title: "Distributed E-Commerce Cloud Infrastructure Migration",
            description: "Design and implement a fully automated microservices deployment pipeline using Docker and PostgreSQL backend clusters.",
            providerId: defaultProviderUser.Id,
            majorId: sampleMajorId,
            specialtyId: sampleSpecialtyId,
            createdAt: DateTime.UtcNow.AddDays(-5)
        );

        if (seededSkills.Any())
        {
            cloudTemplate.AddSkill(seededSkills.First(s => s.Name.Contains("Docker")).Id);
            cloudTemplate.AddSkill(seededSkills.First(s => s.Name.Contains("PostgreSQL")).Id);
        }

        // Milestone 1 (WBS: 40%, Grading: 20%) [source: 2]
        cloudTemplate.AddMilestone(
            title: "Research & Requirements Specification",
            description: "Analyze system bottlenecks and formulate a complete cloud schema migration roadmap.",
            expectedEffortInHours: 15.5m,
            wbsWeight: 40.00m,
            gradingWeight: 20.00m
        );
        var cloudMilestone1 = cloudTemplate.GlobalMilestones.First(m => m.Title == "Research & Requirements Specification");
        cloudTemplate.AddGlobalTaskToMilestone(cloudMilestone1.Id, "Literature Review", "Evaluate standard containerization practices.", 50.00m, DeliverableType.File);
        cloudTemplate.AddGlobalTaskToMilestone(cloudMilestone1.Id, "Use Case Specification", "Outline data durability scenarios.", 50.00m, DeliverableType.Text);

        // Milestone 2 (WBS: 60%, Grading: 80%) [source: 2]
        cloudTemplate.AddMilestone(
            title: "Core Implementation & Thesis Synthesis",
            description: "Deploy production-ready cluster configurations and compile structural defense documentation.",
            expectedEffortInHours: 30.0m,
            wbsWeight: 60.00m,
            gradingWeight: 80.00m
        );
        var cloudMilestone2 = cloudTemplate.GlobalMilestones.First(m => m.Title == "Core Implementation & Thesis Synthesis");
        cloudTemplate.AddGlobalTaskToMilestone(cloudMilestone2.Id, "API Development", "Construct robust database access pipelines.", 70.00m, DeliverableType.Url);
        cloudTemplate.AddGlobalTaskToMilestone(cloudMilestone2.Id, "Final Thesis Submission", "Publish finalized project codebase metrics.", 30.00m, DeliverableType.File);

        cloudTemplate.SubmitForReview();
        cloudTemplate.Approve();

        await context.ProjectTemplates.AddAsync(cloudTemplate);

        // =========================================================================
        // --- Template 2: Predictive Analytics --- [source: 2]
        // =========================================================================
        var analyticsTemplate = new ProjectTemplate(
            title: "Enterprise Predictive Analytics Dashboard Engine",
            description: "Develop an end-to-end predictive analytics data pipeline using Python ML modeling, served via an interactive React TypeScript tracking interface.",
            providerId: defaultProviderUser.Id,
            majorId: sampleMajorId,
            specialtyId: sampleSpecialtyId,
            createdAt: DateTime.UtcNow.AddDays(-3)
        );

        if (seededSkills.Any())
        {
            analyticsTemplate.AddSkill(seededSkills.First(s => s.Name.Contains("Python")).Id);
            analyticsTemplate.AddSkill(seededSkills.First(s => s.Name.Contains("React")).Id);
        }

        // Milestone 1 (WBS: 40%, Grading: 20%) [source: 2]
        analyticsTemplate.AddMilestone(
            title: "Research & Requirements Blueprint",
            description: "Survey predictive variance paradigms and capture business logic rules metrics.",
            expectedEffortInHours: 22.0m,
            wbsWeight: 40.00m,
            gradingWeight: 20.00m
        );
        var analyticsMilestone1 = analyticsTemplate.GlobalMilestones.First(m => m.Title == "Research & Requirements Blueprint");
        analyticsTemplate.AddGlobalTaskToMilestone(analyticsMilestone1.Id, "Literature Review", "Document feature selection frameworks.", 50.00m, DeliverableType.File);
        analyticsTemplate.AddGlobalTaskToMilestone(analyticsMilestone1.Id, "Use Case Specification", "Formulate mathematical model bounds.", 50.00m, DeliverableType.Text);

        // Milestone 2 (WBS: 60%, Grading: 80%) [source: 2]
        analyticsTemplate.AddMilestone(
            title: "Core Implementation & Thesis Delivery",
            description: "Train active analytics model iterations and connect modern graphical presentation dashboards.",
            expectedEffortInHours: 40.0m,
            wbsWeight: 60.00m,
            gradingWeight: 80.00m
        );
        var analyticsMilestone2 = analyticsTemplate.GlobalMilestones.First(m => m.Title == "Core Implementation & Thesis Delivery");
        analyticsTemplate.AddGlobalTaskToMilestone(analyticsMilestone2.Id, "API Development", "Expose secure high-performance serialization sockets.", 70.00m, DeliverableType.Url);
        analyticsTemplate.AddGlobalTaskToMilestone(analyticsMilestone2.Id, "Final Thesis Submission", "Upload verified engine telemetry files.", 30.00m, DeliverableType.File);

        analyticsTemplate.SubmitForReview();
        analyticsTemplate.Approve();

        await context.ProjectTemplates.AddAsync(analyticsTemplate);

        // =========================================================================
        // --- Template 3: Advanced Core Infrastructure Orchestration --- [source: 2]
        // =========================================================================
        var infraTemplate = new ProjectTemplate(
            title: "Enterprise Multi-Region Infrastructure Layer Orchestration",
            description: "Design, build, and evaluate a resilient multi-region core infrastructure platform comprising secure VPC topologies, automated state matrix databases, load-balanced compute node fleets, GitOps deployment automation, and telemetry tracing pipelines.",
            providerId: defaultProviderUser.Id,
            majorId: sampleMajorId,
            specialtyId: sampleSpecialtyId,
            createdAt: DateTime.UtcNow.AddDays(-1)
        );

        if (seededSkills.Any())
        {
            infraTemplate.AddSkill(seededSkills.First(s => s.Name.Contains("Docker")).Id);
            infraTemplate.AddSkill(seededSkills.First(s => s.Name.Contains("PostgreSQL")).Id);
        }

        // Milestone 1 -> WBS: 15.00%, Grading: 10.00% [source: 2]
        infraTemplate.AddMilestone(
            title: "Infrastructure Base Network & Security Provisioning",
            description: "Establish base cloud networks, IAM boundary access rules, and secure multi-region cross-connections.",
            expectedEffortInHours: 20.0m,
            wbsWeight: 15.00m,
            gradingWeight: 10.00m
        );
        var infraM1 = infraTemplate.GlobalMilestones.First(m => m.Title == "Infrastructure Base Network & Security Provisioning");
        infraTemplate.AddGlobalTaskToMilestone(infraM1.Id, "VPC & Subnet Topologies Setup", "Configure multi-region virtual private spaces and network segments.", 50.00m, DeliverableType.Url);
        infraTemplate.AddGlobalTaskToMilestone(infraM1.Id, "IAM Access Control Rules Definition", "Formulate security roles, resource permissions, and boundary access levels.", 50.00m, DeliverableType.File);

        // Milestone 2 -> WBS: 15.00%, Grading: 10.00% [source: 2]
        infraTemplate.AddMilestone(
            title: "Distributed Shared Storage & State Matrix Definition",
            description: "Deploy multi-region persistent data stores, cluster replication topologies, and lifecycle synchronization patterns.",
            expectedEffortInHours: 25.0m,
            wbsWeight: 15.00m,
            gradingWeight: 10.00m
        );
        var infraM2 = infraTemplate.GlobalMilestones.First(m => m.Title == "Distributed Shared Storage & State Matrix Definition");
        infraTemplate.AddGlobalTaskToMilestone(infraM2.Id, "Database Cluster Replication Setup", "Provision high-availability distributed engines across diverse geographic zones.", 50.00m, DeliverableType.Url);
        infraTemplate.AddGlobalTaskToMilestone(infraM2.Id, "State Snapshot & Integrity Verification", "Build operational metrics to capture backup storage pipeline resilience.", 50.00m, DeliverableType.Text);

        // Milestone 3 (Depends on M1 SS) -> WBS: 10.00%, Grading: 10.00% [source: 2]
        infraTemplate.AddMilestone(
            title: "Container Fleet Clusters Provisioning",
            description: "Initialize target platform orchestration nodes and optimize control plane topologies to handle scalable work items.",
            expectedEffortInHours: 30.0m,
            wbsWeight: 10.00m,
            gradingWeight: 10.00m
        );
        var infraM3 = infraTemplate.GlobalMilestones.First(m => m.Title == "Container Fleet Clusters Provisioning");
        infraTemplate.AddGlobalTaskToMilestone(infraM3.Id, "Compute Control Plane Configuration", "Instantiate and align highly resilient orchestration engine managers.", 50.00m, DeliverableType.Url);
        infraTemplate.AddGlobalTaskToMilestone(infraM3.Id, "Dynamic Scaling Policy Scripts", "Establish baseline boundaries to balance workload execution nodes seamlessly.", 50.00m, DeliverableType.File);

        // Milestone 4 (Depends on M1 FS, M3 FS) -> WBS: 20.00%, Grading: 25.00% [source: 2]
        infraTemplate.AddMilestone(
            title: "Routing Traffic Proxies & Gateway Configuration",
            description: "Configure layer-7 application load balancers, public edge protection mechanisms, and routing configurations.",
            expectedEffortInHours: 35.0m,
            wbsWeight: 20.00m,
            gradingWeight: 25.00m
        );
        var infraM4 = infraTemplate.GlobalMilestones.First(m => m.Title == "Routing Traffic Proxies & Gateway Configuration");
        infraTemplate.AddGlobalTaskToMilestone(infraM4.Id, "Ingress Controller Implementation", "Expose secure transit boundaries and forward rules down to internal host channels.", 50.00m, DeliverableType.Url);
        infraTemplate.AddGlobalTaskToMilestone(infraM4.Id, "Edge Firewall Security Hardening", "Bind TLS certificates and configure request throttling definitions.", 50.00m, DeliverableType.File);

        // Milestone 5 (Depends on M2 SS, M3 SS) -> WBS: 15.00%, Grading: 15.00% [source: 2]
        infraTemplate.AddMilestone(
            title: "CI/CD GitOps Continuous Deployment Pipeline",
            description: "Link repository webhooks to continuous automated verification systems and release delivery hooks.",
            expectedEffortInHours: 40.0m,
            wbsWeight: 15.00m,
            gradingWeight: 15.00m
        );
        var infraM5 = infraTemplate.GlobalMilestones.First(m => m.Title == "CI/CD GitOps Continuous Deployment Pipeline");
        infraTemplate.AddGlobalTaskToMilestone(infraM5.Id, "GitOps Orchestration Engine Design", "Construct declarative pipeline configurations to automate environment synchronization.", 50.00m, DeliverableType.File);
        infraTemplate.AddGlobalTaskToMilestone(infraM5.Id, "Automated Environment Promotion Mapping", "Integrate automated health check validation triggers post deployment releases.", 50.00m, DeliverableType.Url);

        // Milestone 6 (Depends on M5 FS) -> WBS: 12.00%, Grading: 15.00% [source: 2]
        infraTemplate.AddMilestone(
            title: "Central Logging & Monitoring Observability Matrix",
            description: "Deploy log aggregate aggregators and collectors to capture telemetry patterns and surface dashboards.",
            expectedEffortInHours: 20.0m,
            wbsWeight: 12.00m,
            gradingWeight: 15.00m
        );
        var infraM6 = infraTemplate.GlobalMilestones.First(m => m.Title == "Central Logging & Monitoring Observability Matrix");
        infraTemplate.AddGlobalTaskToMilestone(infraM6.Id, "Telemetry Agent Instrumentation", "Embed performance tracking daemons onto active fleet clusters.", 50.00m, DeliverableType.Url);
        infraTemplate.AddGlobalTaskToMilestone(infraM6.Id, "Alert Notification Routing Configurations", "Formulate system breach alerts and link channels to administration queues.", 50.00m, DeliverableType.Text);

        // Milestone 7 (Depends on M5 FS) -> WBS: 13.00%, Grading: 15.00% [source: 2]
        infraTemplate.AddMilestone(
            title: "Disaster Recovery Testing & Governance Report",
            description: "Conduct active network disruption chaos assessments, compute recovery metrics, and summarize standard runs.",
            expectedEffortInHours: 25.0m,
            wbsWeight: 13.00m,
            gradingWeight: 15.00m
        );
        var infraM7 = infraTemplate.GlobalMilestones.First(m => m.Title == "Disaster Recovery Testing & Governance Report");
        infraTemplate.AddGlobalTaskToMilestone(infraM7.Id, "Chaos Engineering Failure Simulation", "Execute live cluster partition scenarios to evaluate structural storage recovery metrics.", 50.00m, DeliverableType.File);
        infraTemplate.AddGlobalTaskToMilestone(infraM7.Id, "Architectural Thesis Performance Summary", "Compile a complete performance summary documenting network recovery metrics.", 50.00m, DeliverableType.File);

        // Dependencies [source: 2]
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
        await context.SaveChangesAsync();

        // =========================================================================
        // --- Live Project Workspace Instance --- [source: 2]
        // =========================================================================
        if (!await context.ProjectInstances.AnyAsync())
        {
            var milestoneFactory = new LocalMilestoneFactory();
            var executionClockSnapshot = DateTime.UtcNow;

            var projectInstance = cloudTemplate.Instantiate(
                studentId: defaultStudentUser.Id,
                createdAt: executionClockSnapshot,
                milestoneFactory: milestoneFactory,
                initialRequestedProfessorId: defaultProfessorUser.Id
            );

            var pendingInvitation = projectInstance.SupervisionRequests.First();
            projectInstance.ReviewSupervisionRequest(
                requestId: pendingInvitation.Id,
                accept: true,
                rejectionReason: null,
                reviewedAt: executionClockSnapshot
            );

            var professorToUpdate = await context.Professors.FindAsync(defaultProfessorUser.Id);
            professorToUpdate?.IncrementActiveProjects();

            await context.ProjectInstances.AddAsync(projectInstance);
            await context.SaveChangesAsync();
        }
    }
}
using AcademicGateway.Infrastructure.Identity;
using AcademicGateway.Infrastructure.Persistence.Context;
using AcademicGateway.Domain.Curriculum;
using AcademicGateway.Domain.Skills;
using AcademicGateway.Domain.Reviewers;
using AcademicGateway.Domain.Providers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AcademicGateway.Domain.Students;
using AcademicGateway.Domain.ProjectTemplates;
using AcademicGateway.Domain.Professors;
using AcademicGateway.Domain.ProjectInstances.Services;
using AcademicGateway.Domain.Common.Enums;

namespace AcademicGateway.Infrastructure.Persistence.Context;

/// <summary>
/// Core structural data seeder executing framework role allocations, system-level lookup values, 
/// and administrative testing profiles inside development environments.
/// </summary>
public static class ApplicationDbContextSeed
{
    /// <summary>
    /// Evaluates and seeds essential infrastructure credentials, lookup dictionary matrix tables, and testing profiles.
    /// </summary>
    public static async Task SeedDefaultUserAndDataAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ApplicationDbContext context)
    {
        // 1. Seed System Roles utilizing Explicit Guid configuration types
        string[] roles = { "Administrator", "Reviewer", "Student", "Provider", "Professor", "TechSupport" };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }

        // 2. Seed Default Reviewer Identity Account
        var defaultReviewerEmail = "reviewer@academicgateway.com";
        var defaultReviewerUser = await userManager.FindByEmailAsync(defaultReviewerEmail);

        if (defaultReviewerUser == null)
        {
            defaultReviewerUser = new ApplicationUser
            {
                UserName = defaultReviewerEmail,
                Email = defaultReviewerEmail,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(defaultReviewerUser, "GatewayReviewer123!");
            await userManager.AddToRoleAsync(defaultReviewerUser, "Reviewer");
        }

        // 3. Seed Default Reviewer Domain Profile Entity
        if (!await context.Reviewers.AnyAsync(r => r.Id == defaultReviewerUser.Id))
        {
            var reviewerProfile = new Reviewer(
                id: defaultReviewerUser.Id,
                fullName: "Internal Platform Reviewer"
            );

            await context.Reviewers.AddAsync(reviewerProfile);
            await context.SaveChangesAsync();
        }

        // 4. Seed Default Provider Identity Account (For Onboarding Workflow Validation)
        var defaultProviderEmail = "partner@acmesolutions.internal";
        var defaultProviderUser = await userManager.FindByEmailAsync(defaultProviderEmail);

        if (defaultProviderUser == null)
        {
            defaultProviderUser = new ApplicationUser
            {
                UserName = defaultProviderEmail,
                Email = defaultProviderEmail,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(defaultProviderUser, "CorporatePartner123!");
            await userManager.AddToRoleAsync(defaultProviderUser, "Provider");
        }

        // 5. Seed Default Unverified Provider Domain Profile Entity
        if (!await context.Providers.AnyAsync(p => p.Id == defaultProviderUser.Id))
        {
            var providerProfile = new Provider(
                id: defaultProviderUser.Id,
                companyName: "Acme Corporate Innovations"
            );

            providerProfile.UpdateProfileDetails(
                description: "Global enterprise specializing in cloud computing, infrastructure architecture, and technical software solutions.",
                websiteUrl: "https://acme-innovations.internal"
            );

            await context.Providers.AddAsync(providerProfile);
            await context.SaveChangesAsync();
        }

        // 6. Seed a Pending Onboarding Application to populate the Reviewer Dashboards instantly
        if (!await context.ProviderApplications.AnyAsync(a => a.ProviderId == defaultProviderUser.Id))
        {
            var pendingApplication = new ProviderApplication(
                providerId: defaultProviderUser.Id,
                companyDetails: "Acme is seeking platform verification to sponsor high-scale distributed systems design and microservice architecture projects for final-year computer science tracks.",
                verificationDocumentsUrl: "https://storage.academicgateway.internal/onboarding/acme-credentials.pdf",
                createdAt: DateTime.UtcNow.AddDays(-2) // Submitted 2 days ago to simulate realistic operational backlog
            );

            // Transition the application out of Draft and straight into the Reviewer operational pool
            pendingApplication.SubmitForReview();

            await context.ProviderApplications.AddAsync(pendingApplication);
            await context.SaveChangesAsync();
        }

        // 7. Seed Base Lookup Skills via its behavior-driven constructor rules
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

        // 8. Seed Majors and Specialties with their respective domain-driven design rules
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

        // 9. Seed Default Professor Identity Account
        var defaultProfessorEmail = "professor@academicgateway.com";
        var defaultProfessorUser = await userManager.FindByEmailAsync(defaultProfessorEmail);

        if (defaultProfessorUser == null)
        {
            defaultProfessorUser = new ApplicationUser
            {
                UserName = defaultProfessorEmail,
                Email = defaultProfessorEmail,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(defaultProfessorUser, "GatewayProfessor123!");
            await userManager.AddToRoleAsync(defaultProfessorUser, "Professor");
        }

        // 10. Seed Default Professor Domain Profile Entity
        if (!await context.Professors.AnyAsync(p => p.Id == defaultProfessorUser.Id))
        {
            var professorProfile = new Professor(
                id: defaultProfessorUser.Id,
                fullName: "Dr. Alan Turing",
                department: "Computer Science",
                rank: "Full Professor",
                maxSupervisionCapacity: 5
            );

            professorProfile.UpdateAboutMe("Passionate about distributed systems, theoretical computer science, and advising innovative capstone projects.");

            await context.Professors.AddAsync(professorProfile);
            await context.SaveChangesAsync();
        }

        // 11. Seed Default Student Identity Account
        var defaultStudentEmail = "student@academicgateway.com";
        var defaultStudentUser = await userManager.FindByEmailAsync(defaultStudentEmail);

        if (defaultStudentUser == null)
        {
            defaultStudentUser = new ApplicationUser
            {
                UserName = defaultStudentEmail,
                Email = defaultStudentEmail,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(defaultStudentUser, "GatewayStudent123!");
            await userManager.AddToRoleAsync(defaultStudentUser, "Student");
        }

        // 12. Seed Default Student Domain Profile Entity
        if (!await context.Students.AnyAsync(s => s.Id == defaultStudentUser.Id))
        {
            var studentProfile = new Student(
                id: defaultStudentUser.Id,
                fullName: "Jane Doe",
                graduationYear: 2027
            );

            studentProfile.UpdateAboutMe("Final-year Computer Science student specializing in Software Engineering, interested in backend C# development and cloud architecture.");

            // A. Query a seeded Major along with its Specialties using eager loading (.Include)
            var computerScienceMajor = await context.Majors
                .Include(m => m.Specialties)
                .FirstOrDefaultAsync(m => m.Name == "Computer Science");

            if (computerScienceMajor != null)
            {
                // Assign the Major
                studentProfile.AddMajor(computerScienceMajor.Id);

                // Extract and assign a Specialty under that Major (e.g., Software Engineering)
                var softwareEngineeringSpecialty = computerScienceMajor.Specialties
                    .FirstOrDefault(s => s.Name == "Software Engineering");

                if (softwareEngineeringSpecialty != null)
                {
                    studentProfile.AddSpecialty(softwareEngineeringSpecialty.Id);
                }
            }

            // B. Query a couple of seeded baseline Skills to assign to the student
            var dotNetSkill = await context.Skills
                .FirstOrDefaultAsync(s => s.Name == "C# .NET Backend Development");

            var dockerSkill = await context.Skills
                .FirstOrDefaultAsync(s => s.Name == "Docker Containerization");

            if (dotNetSkill != null)
            {
                studentProfile.AddSkill(dotNetSkill.Id);
            }

            if (dockerSkill != null)
            {
                studentProfile.AddSkill(dockerSkill.Id);
            }

            // C. Save the fully populated Student aggregate root profile context
            await context.Students.AddAsync(studentProfile);
            await context.SaveChangesAsync();
        }

        // 13. Seed Baseline Project Templates & 1 Live Running Instance Workspace Track
        if (!await context.ProjectTemplates.AnyAsync())
        {
            var seededSkills = await context.Skills.ToListAsync();

            // Retrieve academic alignment references for seeding project templates
            var computerScienceMajor = await context.Majors
                .Include(m => m.Specialties)
                .FirstOrDefaultAsync(m => m.Name == "Computer Science");

            Guid? sampleMajorId = computerScienceMajor?.Id;
            Guid? sampleSpecialtyId = computerScienceMajor?.Specialties
                .FirstOrDefault(s => s.Name == "Software Engineering")?.Id;

            // =========================================================================
            // --- Template 1: Cloud Migration ---
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

            // Milestone 1 (WBS: 40%, Grading: 20%)
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

            // Milestone 2 (WBS: 60%, Grading: 80%)
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
            // --- Template 2: Predictive Analytics ---
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

            // Milestone 1 (WBS: 40%, Grading: 20%)
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

            // Milestone 2 (WBS: 60%, Grading: 80%)
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
            // --- Template 3: Advanced Core Infrastructure Orchestration (Complicated DAG Layout) ---
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

            // Milestone 1 (Independent) -> WBS: 15.00%, Grading: 10.00%
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

            // Milestone 2 (Independent) -> WBS: 15.00%, Grading: 10.00%
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

            // Milestone 3 (Depends on M1 SS) -> WBS: 10.00%, Grading: 10.00%
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

            // Milestone 4 (Depends on M1 FS, M3 FS) -> WBS: 20.00%, Grading: 25.00%
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

            // Milestone 5 (Depends on M2 SS, M3 SS) -> WBS: 15.00%, Grading: 15.00%
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

            // Milestone 6 (Depends on M5 FS) -> WBS: 12.00%, Grading: 15.00%
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

            // Milestone 7 (Depends on M5 FS) -> WBS: 13.00%, Grading: 15.00%
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

            // Establish Explicit Scheduling Graph Nodes Relationships
            // M3 depends on M1 start to start
            infraTemplate.AddMilestoneDependency(infraM3.Id, infraM1.Id, DependencyType.StartToStart);

            // M4 depends on M1 and M3 end to start (FinishToStart)
            infraTemplate.AddMilestoneDependency(infraM4.Id, infraM1.Id, DependencyType.FinishToStart);
            infraTemplate.AddMilestoneDependency(infraM4.Id, infraM3.Id, DependencyType.FinishToStart);

            // M5 depends on M2 and M3 start to start
            infraTemplate.AddMilestoneDependency(infraM5.Id, infraM2.Id, DependencyType.StartToStart);
            infraTemplate.AddMilestoneDependency(infraM5.Id, infraM3.Id, DependencyType.StartToStart);

            // M6 depends on M5 end to start (FinishToStart)
            infraTemplate.AddMilestoneDependency(infraM6.Id, infraM5.Id, DependencyType.FinishToStart);

            // M7 depends on M5 end to start (FinishToStart)
            infraTemplate.AddMilestoneDependency(infraM7.Id, infraM5.Id, DependencyType.FinishToStart);

            // Submit and Approve through Domain State Transition Pool
            infraTemplate.SubmitForReview();
            infraTemplate.Approve();

            await context.ProjectTemplates.AddAsync(infraTemplate);

            // Commit all templates seamlessly to the database track
            await context.SaveChangesAsync();

            // =========================================================================
            // 14. Instantiate Live Project Workspace utilizing Prototype Patterns
            // =========================================================================
            if (!await context.ProjectInstances.AnyAsync())
            {
                var milestoneFactory = new LocalMilestoneFactory();
                var executionClockSnapshot = DateTime.UtcNow;

                // Manufacture clean running channel context with a tracking supervisor invitation initialized
                // Internally invokes LocalMilestoneFactory to clone milestones and match corresponding LocalTasks
                var projectInstance = cloudTemplate.Instantiate(
                    studentId: defaultStudentUser.Id,
                    createdAt: executionClockSnapshot,
                    milestoneFactory: milestoneFactory,
                    initialRequestedProfessorId: defaultProfessorUser.Id
                );

                // Isolate and accept the generated matchmaking invitation to safely step State machine indexes into Active mode
                var pendingInvitation = projectInstance.SupervisionRequests.First();
                projectInstance.ReviewSupervisionRequest(
                    requestId: pendingInvitation.Id,
                    accept: true,
                    rejectionReason: null,
                    reviewedAt: executionClockSnapshot
                );

                // Synchronize and update current active supervision loads on the Professor Aggregate Root
                var professorToUpdate = await context.Professors.FindAsync(defaultProfessorUser.Id);
                professorToUpdate?.IncrementActiveProjects();

                // Save both mutated aggregates seamlessly inside the unit-of-work pipeline transaction
                await context.ProjectInstances.AddAsync(projectInstance);
                await context.SaveChangesAsync();
            }
        }

        // =========================================================================
        // --- 15: Seed Verified Provider & Technical Support Account for Testing ---
        // =========================================================================
        var verifiedProviderEmail = "verified-partner@cloudsystems.internal";
        var verifiedProviderUser = await userManager.FindByEmailAsync(verifiedProviderEmail);

        if (verifiedProviderUser == null)
        {
            verifiedProviderUser = new ApplicationUser
            {
                UserName = verifiedProviderEmail,
                Email = verifiedProviderEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(verifiedProviderUser, "VerifiedPartner123!");
            await userManager.AddToRoleAsync(verifiedProviderUser, "Provider");
        }

        if (!await context.Providers.AnyAsync(p => p.Id == verifiedProviderUser.Id))
        {
            var verifiedProviderProfile = new Provider(
                id: verifiedProviderUser.Id,
                companyName: "Cloud Systems Architectures"
            );
            verifiedProviderProfile.UpdateProfileDetails(
                description: "Verified infrastructure firm specializing in hyper-scale cluster engineering guidance.",
                websiteUrl: "https://cloudsystems.internal"
            );

            // Explicitly bypass the review funnel for this development sandbox test partner
            verifiedProviderProfile.VerifyProfile();

            await context.Providers.AddAsync(verifiedProviderProfile);
            await context.SaveChangesAsync();
        }

        // Now seed a corresponding Tech Support user under their organization boundary
        var techSupportEmail = "mentor.alan@cloudsystems.internal";
        var techSupportUser = await userManager.FindByEmailAsync(techSupportEmail);

        if (techSupportUser == null)
        {
            techSupportUser = new ApplicationUser
            {
                UserName = techSupportEmail,
                Email = techSupportEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(techSupportUser, "SecureTechSupport123!");
            await userManager.AddToRoleAsync(techSupportUser, "TechSupport");
        }

        if (!await context.TechSupportAccounts.AnyAsync(ts => ts.Id == techSupportUser.Id))
        {
            var techSupportAccount = new TechSupportAccount(
                id: techSupportUser.Id,
                providerId: verifiedProviderUser.Id,
                staffNumber: "CS-G9-011",
                supportTier: "Tier 3 Systems Architect"
            );

            await context.TechSupportAccounts.AddAsync(techSupportAccount);
            await context.SaveChangesAsync();
        }
    }
}
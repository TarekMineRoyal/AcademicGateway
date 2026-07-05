using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Infrastructure.Identity;
using AcademicGateway.Domain.Curriculum;
using AcademicGateway.Domain.Professors;
using AcademicGateway.Domain.ProjectTemplates;
using AcademicGateway.Domain.ProjectInstances;
using AcademicGateway.Domain.Providers;
using AcademicGateway.Domain.Reviewers;
using AcademicGateway.Domain.Skills;
using AcademicGateway.Domain.Students;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;

namespace AcademicGateway.Infrastructure.Persistence.Context;

/// <summary>
/// Core database context implementation supporting Identity and multi-aggregate profile extensions.
/// Acts purely as a transactional Unit of Work tracking entity states, completely decoupled from operational pipeline side effects.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContext"/> with required persistence options.
    /// </summary>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // ==========================================
    // DbSets for Domain Entities & Aggregate Roots
    // ==========================================
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<Professor> Professors => Set<Professor>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<StudentSkill> StudentSkills => Set<StudentSkill>();
    public DbSet<Major> Majors => Set<Major>();
    public DbSet<Specialty> Specialties => Set<Specialty>();
    public DbSet<StudentMajor> StudentMajors => Set<StudentMajor>();
    public DbSet<StudentSpecialty> StudentSpecialties => Set<StudentSpecialty>();

    // Faculty & Lookup Extensions
    public DbSet<ResearchInterest> ResearchInterests => Set<ResearchInterest>();
    public DbSet<ProfessorResearchInterest> ProfessorResearchInterests => Set<ProfessorResearchInterest>();

    // Core Workflow Mappings
    public DbSet<Reviewer> Reviewers => Set<Reviewer>();
    public DbSet<ProviderApplication> ProviderApplications => Set<ProviderApplication>();
    public DbSet<ProjectTemplate> ProjectTemplates => Set<ProjectTemplate>();
    public DbSet<ProjectTemplateSkill> ProjectTemplateSkills => Set<ProjectTemplateSkill>();
    public DbSet<TechSupportAccount> TechSupportAccounts => Set<TechSupportAccount>();

    // Execution Subsystem & Grading Graph Mappings
    public DbSet<ProjectInstance> ProjectInstances => Set<ProjectInstance>();
    public DbSet<LocalMilestone> LocalMilestones => Set<LocalMilestone>();
    public DbSet<MilestoneComment> MilestoneComments => Set<MilestoneComment>();
    public DbSet<SupervisionRequest> SupervisionRequests => Set<SupervisionRequest>();
    public DbSet<TechSupportProposal> TechSupportProposals => Set<TechSupportProposal>();

    /// <summary>
    /// Configures structural model mappings via automated reflection-based assembly discovery.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Apply base ASP.NET Core Identity table mapping keys first
        base.OnModelCreating(builder);

        // Automatically scan, instantiate, and register every single IEntityTypeConfiguration class 
        // isolated inside the Persistence/Configurations directory boundary
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Infrastructure.Identity;
using Domain.Curriculum;
using Domain.Professors;
using Domain.ProjectTemplates;
using Domain.Providers;
using Domain.Skills;
using Domain.Students;
using Domain.SystemStaff;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace AcademicGateway.Infrastructure.Persistence;

// Inherits from IdentityDbContext supporting custom Guid types for keys
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets for our Domain Entities
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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Crucial: Identity needs its own internal mappings configured first
        base.OnModelCreating(builder);

        // ==========================================
        // 1-to-1 Profile Extension Configurations
        // ==========================================

        // Student Configuration
        builder.Entity<Student>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.HasOne<ApplicationUser>()
                  .WithOne()
                  .HasForeignKey<Student>(s => s.Id)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Provider Configuration
        builder.Entity<Provider>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasOne<ApplicationUser>()
                  .WithOne()
                  .HasForeignKey<Provider>(p => p.Id)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Professor Configuration
        builder.Entity<Professor>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasOne<ApplicationUser>()
                  .WithOne()
                  .HasForeignKey<Professor>(p => p.Id)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Reviewer Configuration (Unified to 1:1 Primary Key Strategy)
        builder.Entity<Reviewer>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasOne<ApplicationUser>()
                  .WithOne()
                  .HasForeignKey<Reviewer>(r => r.Id)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Tech Support Account Configuration (Unified to 1:1 Primary Key Strategy)
        builder.Entity<TechSupportAccount>(entity =>
        {
            entity.HasKey(ts => ts.Id);
            entity.HasOne<ApplicationUser>()
                  .WithOne()
                  .HasForeignKey<TechSupportAccount>(ts => ts.Id)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ==========================================
        // Many-to-Many Configuration: Student Relations
        // ==========================================

        builder.Entity<StudentSkill>(entity =>
        {
            entity.HasKey(ss => new { ss.StudentId, ss.SkillId });

            entity.HasOne(ss => ss.Student)
                  .WithMany(s => s.StudentSkills)
                  .HasForeignKey(ss => ss.StudentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ss => ss.Skill)
                  .WithMany(s => s.StudentSkills)
                  .HasForeignKey(ss => ss.SkillId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<StudentMajor>(entity =>
        {
            entity.HasKey(sm => new { sm.StudentId, sm.MajorId });

            entity.HasOne(sm => sm.Student)
                  .WithMany(s => s.StudentMajors)
                  .HasForeignKey(sm => sm.StudentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sm => sm.Major)
                  .WithMany(m => m.StudentMajors)
                  .HasForeignKey(sm => sm.MajorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<StudentSpecialty>(entity =>
        {
            entity.HasKey(ss => new { ss.StudentId, ss.SpecialtyId });

            entity.HasOne(ss => ss.Student)
                  .WithMany(s => s.StudentSpecialties)
                  .HasForeignKey(ss => ss.StudentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ss => ss.Specialty)
                  .WithMany(s => s.StudentSpecialties)
                  .HasForeignKey(ss => ss.SpecialtyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ==========================================
        // Many-to-Many Configuration: Professor Relations
        // ==========================================

        builder.Entity<ProfessorResearchInterest>(entity =>
        {
            entity.HasKey(pri => new { pri.ProfessorId, pri.ResearchInterestId });

            entity.HasOne(pri => pri.Professor)
                  .WithMany(p => p.ProfessorResearchInterests)
                  .HasForeignKey(pri => pri.ProfessorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pri => pri.ResearchInterest)
                  .WithMany(r => r.ProfessorResearchInterests)
                  .HasForeignKey(pri => pri.ResearchInterestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ==========================================
        // 1-to-Many Configuration: Curriculum
        // ==========================================

        builder.Entity<Specialty>(entity =>
        {
            entity.HasOne(s => s.Major)
                  .WithMany(m => m.Specialties)
                  .HasForeignKey(s => s.MajorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ==========================================
        // Core Workflow Funnel Configurations
        // ==========================================

        // Provider Application Mappings
        builder.Entity<ProviderApplication>(entity =>
        {
            entity.HasKey(pa => pa.Id);

            entity.HasOne(pa => pa.Provider)
                  .WithMany()
                  .HasForeignKey(pa => pa.ProviderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pa => pa.ReviewedBy)
                  .WithMany(r => r.ReviewedApplications)
                  .HasForeignKey(pa => pa.ReviewedById)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Project Template Mappings
        builder.Entity<ProjectTemplate>(entity =>
        {
            entity.HasKey(pt => pt.Id);

            entity.HasOne(pt => pt.Provider)
                  .WithMany(p => p.ProjectTemplates)
                  .HasForeignKey(pt => pt.ProviderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Project Template Skills Join Mappings
        builder.Entity<ProjectTemplateSkill>(entity =>
        {
            entity.HasKey(pts => new { pts.ProjectTemplateId, pts.SkillId });

            entity.HasOne(pts => pts.ProjectTemplate)
                  .WithMany(t => t.ProjectTemplateSkills)
                  .HasForeignKey(pts => pts.ProjectTemplateId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pts => pts.Skill)
                  .WithMany()
                  .HasForeignKey(pts => pts.SkillId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
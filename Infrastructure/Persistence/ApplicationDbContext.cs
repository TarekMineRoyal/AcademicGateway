using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Infrastructure.Identity;
using Domain.Lookups;
using Domain.Professors;
using Domain.ProjectTemplates;
using Domain.Providers;
using Domain.Students;
using Domain.SystemStaff;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AcademicGateway.Infrastructure.Persistence;

// Inherits from IdentityDbContext using our custom ApplicationUser
// AND implements the IApplicationDbContext interface for the Application layer
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
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

    // New Mini-Sprint 2 DbSets
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
        // 1-to-1 Profile Configurations
        // ==========================================

        // Student Configuration
        builder.Entity<Student>(entity =>
        {
            entity.HasKey(s => s.UserId);
            entity.HasOne<ApplicationUser>()
                  .WithOne()
                  .HasForeignKey<Student>(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Provider Configuration
        builder.Entity<Provider>(entity =>
        {
            entity.HasKey(p => p.UserId);
            entity.HasOne<ApplicationUser>()
                  .WithOne()
                  .HasForeignKey<Provider>(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Professor Configuration
        builder.Entity<Professor>(entity =>
        {
            entity.HasKey(p => p.UserId);
            entity.HasOne<ApplicationUser>()
                  .WithOne()
                  .HasForeignKey<Professor>(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Reviewer Configuration
        builder.Entity<Reviewer>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasOne<ApplicationUser>()
                  .WithOne()
                  .HasForeignKey<Reviewer>(r => r.IdentityUserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ==========================================
        // Many-to-Many Configuration: StudentSkills
        // ==========================================

        builder.Entity<StudentSkill>(entity =>
        {
            // Composite Primary Key
            entity.HasKey(ss => new { ss.StudentId, ss.SkillId });

            // Relationship to Student
            entity.HasOne(ss => ss.Student)
                  .WithMany(s => s.StudentSkills)
                  .HasForeignKey(ss => ss.StudentId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Relationship to Skill
            entity.HasOne(ss => ss.Skill)
                  .WithMany(s => s.StudentSkills)
                  .HasForeignKey(ss => ss.SkillId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ==========================================
        // 1-to-Many Configuration: Major to Specialties
        // ==========================================
        builder.Entity<Specialty>(entity =>
        {
            entity.HasOne(s => s.Major)
                  .WithMany(m => m.Specialties)
                  .HasForeignKey(s => s.MajorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ==========================================
        // Many-to-Many Configuration: StudentMajors
        // ==========================================
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

        // ==========================================
        // Many-to-Many Configuration: StudentSpecialties
        // ==========================================
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
        // Mini-Sprint 2 Workflow Entity Configurations
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
                  .WithMany()
                  .HasForeignKey(pa => pa.ReviewedById)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Project Template Mappings
        builder.Entity<ProjectTemplate>(entity =>
        {
            entity.HasKey(pt => pt.Id);

            entity.HasOne(pt => pt.Provider)
                  .WithMany()
                  .HasForeignKey(pt => pt.ProviderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pt => pt.ApprovedBy)
                  .WithMany()
                  .HasForeignKey(pt => pt.ApprovedById)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Project Template Skills Join Table Mappings
        builder.Entity<ProjectTemplateSkill>(entity =>
        {
            entity.HasKey(pts => new { pts.ProjectTemplateId, pts.SkillId });

            entity.HasOne(pts => pts.ProjectTemplate)
                  .WithMany(t => t.TemplateSkills)
                  .HasForeignKey(pts => pts.ProjectTemplateId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pts => pts.Skill)
                  .WithMany()
                  .HasForeignKey(pts => pts.SkillId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Tech Support Account Mappings
        builder.Entity<TechSupportAccount>(entity =>
        {
            entity.HasKey(ts => ts.Id);

            entity.HasOne(ts => ts.Provider)
                  .WithMany()
                  .HasForeignKey(ts => ts.ProviderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<ApplicationUser>()
                  .WithOne()
                  .HasForeignKey<TechSupportAccount>(ts => ts.IdentityUserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
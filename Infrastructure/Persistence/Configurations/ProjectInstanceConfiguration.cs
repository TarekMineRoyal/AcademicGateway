using AcademicGateway.Domain.ProjectInstances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGateway.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mappings, structural invariants, and constraints governing the persistence storage layout of the <see cref="ProjectInstance"/> aggregate root.
/// Enhanced for Sprint 4 to securely persist macro-level aggregate final grading metrics and optional supervisor relationships.
/// </summary>
public class ProjectInstanceConfiguration : IEntityTypeConfiguration<ProjectInstance>
{
    /// <summary>
    /// Configures the relational database table schema layout, column typings, and navigational boundaries for project workspace aggregates.
    /// </summary>
    public void Configure(EntityTypeBuilder<ProjectInstance> builder)
    {
        // Define explicit storage table mapping destination
        builder.ToTable("ProjectInstances");

        // Primary key configuration parameters
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        // Standard identity mapping tracking links
        builder.Property(p => p.StudentId)
            .IsRequired();

        builder.Property(p => p.TemplateId)
            .IsRequired();

        builder.Property(p => p.ProviderId)
            .IsRequired();

        // Enforce baseline string sanitation boundaries for snapshots
        builder.Property(p => p.TitleSnapshot)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.DescriptionSnapshot)
            .IsRequired()
            .HasMaxLength(4000);

        // Enum conversion mapping configurations
        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.EndDate)
            .IsRequired(false);

        // =========================================================================
        // MACRO-LEVEL AGGREGATE FINAL GRADING PROP CONSTRAINTS
        // =========================================================================

        // Configure overall score column to map cleanly onto SQL decimal structures without loss of rounding scale precision
        builder.Property(p => p.OverallGrade)
            .IsRequired(false)
            .HasColumnType("decimal(5,2)");

        builder.Property(p => p.ProjectGradedAt)
            .IsRequired(false);

        // =========================================================================
        // DOMAIN ENCAPSULATION & NAVIGATIONAL RELATIONSHIPS
        // =========================================================================

        // Explicit foreign key configuration mapping to optional Academic Supervisor profile.
        // Set to Restrict so that deleting a professor profile is blocked if they are actively tracking a project.
        builder.HasOne(p => p.Supervisor)
            .WithMany()
            .HasForeignKey(p => p.SupervisorId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Map internal skill snapshot bridging array value table rows
        builder.HasMany(p => p.SnapshotSkills)
            .WithOne()
            .HasForeignKey(ps => ps.ProjectInstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Map historical supervision invitations tracking loop
        builder.HasMany(p => p.SupervisionRequests)
            .WithOne()
            .HasForeignKey(sr => sr.ProjectInstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Map corporate technical mentoring proposals tracking loop
        builder.HasMany(p => p.TechSupportProposals)
            .WithOne()
            .HasForeignKey(tp => tp.ProjectInstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Map child milestone snapshotted graph nodes.
        // Configures backing field direct storage channel parameters to preserve aggregate encapsulation boundaries.
        builder.HasMany(p => p.LocalMilestones)
            .WithOne()
            .HasForeignKey(m => m.ProjectInstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(ProjectInstance.LocalMilestones))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
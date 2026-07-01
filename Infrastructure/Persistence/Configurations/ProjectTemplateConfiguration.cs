using Domain.ProjectTemplates;
using Domain.ProjectTemplates.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGateway.Infrastructure.Persistence.Configurations;

/// <summary>
/// Relational database configuration mapping for the <see cref="ProjectTemplate"/> aggregate root.
/// Enforces domain invariants, property constraints, and cascading lifecycle rules at the persistence tier.
/// </summary>
public class ProjectTemplateConfiguration : IEntityTypeConfiguration<ProjectTemplate>
{
    public void Configure(EntityTypeBuilder<ProjectTemplate> builder)
    {
        // Define the target table name clearly
        builder.ToTable("ProjectTemplates");

        // Identity Mapping
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        // Attribute Constraints
        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(2000);

        // State Machine Enum Conversion
        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (ProjectTemplateStatus)System.Enum.Parse(typeof(ProjectTemplateStatus), v))
            .HasMaxLength(50);

        builder.Property(x => x.ReviewerFeedback)
            .IsRequired(false)
            .HasMaxLength(1000);

        // Relationship back to the Provider aggregate root
        builder.HasOne(x => x.Provider)
            .WithMany(p => p.ProjectTemplates)
            .HasForeignKey(x => x.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Encapsulated Aggregate Collection Mapping
        // Accesses the internal backing field directly bypassing the public IReadOnlyCollection exposure
        builder.HasMany(x => x.ProjectTemplateSkills)
            .WithOne()
            .HasForeignKey(x => x.ProjectTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(ProjectTemplate.ProjectTemplateSkills))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

/// <summary>
/// Relational database configuration mapping for the <see cref="ProjectTemplateSkill"/> join entity.
/// Establishes composite index clustering and foreign key associations with strict delete restrictions.
/// </summary>
public class ProjectTemplateSkillConfiguration : IEntityTypeConfiguration<ProjectTemplateSkill>
{
    public void Configure(EntityTypeBuilder<ProjectTemplateSkill> builder)
    {
        builder.ToTable("ProjectTemplateSkills");

        // Composite primary key configuration for the many-to-many lookup relationship
        builder.HasKey(x => new { x.ProjectTemplateId, x.SkillId });

        // Associate with the master Skill lookups safely
        builder.HasOne(x => x.Skill)
            .WithMany()
            .HasForeignKey(x => x.SkillId)
            .OnDelete(DeleteBehavior.Restrict); // Prevents cross-aggregate deletion side effects
    }
}
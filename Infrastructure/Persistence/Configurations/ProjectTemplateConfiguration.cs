using Domain.ProjectTemplates;
using Domain.ProjectTemplates.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGateway.Infrastructure.Persistence.Configurations;

/// <summary>
/// Relational database configuration mapping for the <see cref="ProjectTemplate"/> aggregate root.
/// </summary>
public class ProjectTemplateConfiguration : IEntityTypeConfiguration<ProjectTemplate>
{
    public void Configure(EntityTypeBuilder<ProjectTemplate> builder)
    {
        builder.ToTable("ProjectTemplates");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(2000);

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

        // Configure the collection path down to the join table definitions cleanly
        builder.HasMany(x => x.ProjectTemplateSkills)
            .WithOne(x => x.ProjectTemplate)
            .HasForeignKey(x => x.ProjectTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(ProjectTemplate.ProjectTemplateSkills))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

/// <summary>
/// Relational database configuration mapping for the <see cref="ProjectTemplateSkill"/> join entity.
/// </summary>
public class ProjectTemplateSkillConfiguration : IEntityTypeConfiguration<ProjectTemplateSkill>
{
    public void Configure(EntityTypeBuilder<ProjectTemplateSkill> builder)
    {
        builder.ToTable("ProjectTemplateSkills");

        // Explicitly enforce the composite primary key configuration
        builder.HasKey(x => new { x.ProjectTemplateId, x.SkillId });

        // Associate with the master Skill lookups safely
        builder.HasOne(x => x.Skill)
            .WithMany()
            .HasForeignKey(x => x.SkillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
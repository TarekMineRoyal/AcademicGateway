using AcademicGateway.Domain.ProjectInstances;
using AcademicGateway.Domain.Skills;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGateway.Infrastructure.Persistence.Configurations;

/// <summary>
/// Database configuration mappings for the <see cref="ProjectInstanceSkill"/> join entity.
/// Configures table schemas, composite primary keys, field rules, and multi-entity relational constraints.
/// </summary>
public class ProjectInstanceSkillConfiguration : IEntityTypeConfiguration<ProjectInstanceSkill>
{
    /// <summary>
    /// Configures the database schema boundaries, composite keys, and foreign keys for <see cref="ProjectInstanceSkill"/>.
    /// </summary>
    /// <param name="builder">The builder API used to define the join entity configuration properties.</param>
    public void Configure(EntityTypeBuilder<ProjectInstanceSkill> builder)
    {
        // Define explicit physical database table name mapping
        builder.ToTable("ProjectInstanceSkills");

        // Explicitly map the primitive Guid properties as mandatory columns
        builder.Property(x => x.ProjectInstanceId)
            .IsRequired();

        builder.Property(x => x.SkillId)
            .IsRequired();

        // Define the composite primary key using the mapped immutable columns
        builder.HasKey(x => new { x.ProjectInstanceId, x.SkillId });

        // RELATIONSHIP MAPPING
        builder.HasOne<ProjectInstance>()
            .WithMany(x => x.SnapshotSkills)
            .HasForeignKey(x => x.ProjectInstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Bind the relationship to the Skill lookup table safely
        builder.HasOne<Skill>()
            .WithMany()
            .HasForeignKey(x => x.SkillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
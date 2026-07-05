using AcademicGateway.Domain.ProjectInstances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGateway.Infrastructure.Persistence.Configurations;

/// <summary>
/// Database configuration mappings for the <see cref="ProjectInstance"/> aggregate root boundary.
/// Configures high-level relational matching properties and cascades deletion targets down to the milestone graph.
/// </summary>
public class ProjectInstanceConfiguration : IEntityTypeConfiguration<ProjectInstance>
{
    /// <summary>
    /// Configures the core field parameters and relational hierarchies linked to active student workspaces.
    /// </summary>
    public void Configure(EntityTypeBuilder<ProjectInstance> builder)
    {
        // Define explicit physical database table name
        builder.ToTable("ProjectInstances");

        // Primary Key definition
        builder.HasKey(x => x.Id);

        // Core relational tracking identifiers
        builder.Property(x => x.StudentId)
            .IsRequired();

        builder.Property(x => x.ProviderId)
            .IsRequired();

        builder.Property(x => x.TemplateId)
            .IsRequired();

        // Calendar deadlines and limits
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Execution deadline is nullable on initialization until supervisor assignment
        builder.Property(x => x.EndDate)
            .IsRequired(false);

        // Map the Status enum code strictly to a clean readable string description in SQL rows
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // RELATIONSHIP HIERARCHIES (AGGREGATE BOUNDARY PROTECTION)
        // Binds the active milestone execution graph to the lifetime of the project instance root
        builder.HasMany(x => x.LocalMilestones)
            .WithOne()
            .HasForeignKey(x => x.ProjectInstanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
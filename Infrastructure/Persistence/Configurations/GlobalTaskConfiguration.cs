using AcademicGateway.Domain.ProjectTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGateway.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mappings and structural relational database constraints governing the persistence layout of the <see cref="GlobalTask"/> entity.
/// Governs nesting configurations and schema constraints within a milestone blueprint.
/// </summary>
public class GlobalTaskConfiguration : IEntityTypeConfiguration<GlobalTask>
{
    /// <summary>
    /// Configures the relational database table architecture, keys, field typings, and data transformations for global template tasks.
    /// </summary>
    public void Configure(EntityTypeBuilder<GlobalTask> builder)
    {
        // Define explicit storage table mapping destination
        builder.ToTable("GlobalTasks");

        // Set primary key identifier parameters
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        // Structural relationship identifier mapping
        builder.Property(t => t.GlobalMilestoneId)
            .IsRequired();

        // Conceptual core fields and string length constraints
        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(4000);

        // Operational Work Breakdown Structure (WBS) progress layout allocation
        builder.Property(t => t.Weight)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        // Enum formatting constraints and value conversion transformations
        builder.Property(t => t.RequiredDeliverableType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
    }
}
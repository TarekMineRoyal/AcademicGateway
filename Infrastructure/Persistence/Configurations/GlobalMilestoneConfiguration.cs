using AcademicGateway.Domain.ProjectTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGateway.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mappings and table constraints governing the persistence storage layout of the <see cref="GlobalMilestone"/> template entity.
/// </summary>
public class GlobalMilestoneConfiguration : IEntityTypeConfiguration<GlobalMilestone>
{
    /// <summary>
    /// Configures the relational database table architecture, keys, and operational navigation vectors for global template milestones.
    /// </summary>
    public void Configure(EntityTypeBuilder<GlobalMilestone> builder)
    {
        // Define explicit storage table mapping destination
        builder.ToTable("GlobalMilestones");

        // Set primary key identifier parameters
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .ValueGeneratedNever();

        // Structural metadata constraints
        builder.Property(m => m.ProjectTemplateId)
            .IsRequired();

        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Description)
            .IsRequired()
            .HasMaxLength(4000);

        // Weighted Work Breakdown Structure (WBS) progress and score priority layouts
        builder.Property(m => m.WbsWeight)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        builder.Property(m => m.GradingWeight)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        // Configure the GlobalTasks child collection navigation into its private backing field
        builder.HasMany(m => m.GlobalTasks)
            .WithOne()
            .HasForeignKey(t => t.GlobalMilestoneId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(GlobalMilestone.GlobalTasks))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Performance Optimization Index for Template lookups
        builder.HasIndex(m => m.ProjectTemplateId)
            .HasDatabaseName("IX_GlobalMilestones_ProjectTemplateId");
    }
}
using AcademicGateway.Domain.ProjectInstances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGateway.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mappings and table constraints governing the persistence storage layout of the <see cref="LocalMilestone"/> entity.
/// Enhanced to explicitly map owned dependent chronological constraint validation paths and manage nested operational task workflows.
/// </summary>
public class LocalMilestoneConfiguration : IEntityTypeConfiguration<LocalMilestone>
{
    /// <summary>
    /// Configures the relational database table architecture, keys, field typings, and navigational modes for runtime milestones.
    /// </summary>
    public void Configure(EntityTypeBuilder<LocalMilestone> builder)
    {
        // Define explicit storage table mapping destination
        builder.ToTable("LocalMilestones");

        // Set primary key identifier parameters
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .ValueGeneratedNever();

        // Structural metadata constraints
        builder.Property(m => m.ProjectInstanceId)
            .IsRequired();

        builder.Property(m => m.TitleSnapshot)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.DescriptionSnapshot)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(m => m.ExpectedEffortInHours)
            .IsRequired()
            .HasColumnType("decimal(6,2)");

        // Weighted Work Breakdown Structure (WBS) progress and score priority mappings
        builder.Property(m => m.WbsWeight)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        builder.Property(m => m.GradingWeight)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        // Enum data mapping transformations
        builder.Property(m => m.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // Timeline date properties
        builder.Property(m => m.ScheduledStartDate)
            .IsRequired(false);

        builder.Property(m => m.ScheduledEndDate)
            .IsRequired(false);

        // =========================================================================
        // DEPENDENCY GRAPH WORK INVARIANTS MAPS (OWNED ENTITY)
        // =========================================================================

        // Maps the MilestoneDependency collection as a tightly bound owned table 
        // using a shadow composite primary key layout to cleanly build the DAG link matrix.
        builder.OwnsMany(m => m.InboundDependencies, dep =>
        {
            dep.ToTable("MilestoneDependencies");

            dep.WithOwner()
                .HasForeignKey("LocalMilestoneId");

            // Setup composite tracking key: Parent Node Identifier + Predecessor Node Identifier
            dep.HasKey("LocalMilestoneId", "PredecessorId");

            dep.Property(d => d.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
        });

        // Nested Operational and Academic Tasks Navigation Mapping
        builder.HasMany(m => m.LocalTasks)
            .WithOne()
            .HasForeignKey(t => t.LocalMilestoneId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(LocalMilestone.LocalTasks))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Conversation Encapsulation Settings
        builder.HasMany(m => m.Comments)
            .WithOne()
            .HasForeignKey(c => c.LocalMilestoneId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(LocalMilestone.Comments))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Performance Optimization Index
        builder.HasIndex(m => m.ProjectInstanceId)
            .HasDatabaseName("IX_LocalMilestones_ProjectInstanceId");
    }
}
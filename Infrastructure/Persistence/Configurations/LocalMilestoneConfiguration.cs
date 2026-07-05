using AcademicGateway.Domain.ProjectInstances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGateway.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mappings and table constraints governing the persistence storage layout of the <see cref="LocalMilestone"/> entity.
/// Enhanced for Sprint 4 to manage granular evaluation metrics, grading scopes, and direct backing-field encapsulation for discussion threads.
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

        // Enum data mapping transformations
        builder.Property(m => m.RequiredDeliverableType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(m => m.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // Timeline date properties
        builder.Property(m => m.ScheduledStartDate)
            .IsRequired(false);

        builder.Property(m => m.ScheduledEndDate)
            .IsRequired(false);

        builder.Property(m => m.SubmissionPayload)
            .IsRequired(false)
            .HasMaxLength(4000);

        builder.Property(m => m.SubmittedAt)
            .IsRequired(false);

        // =========================================================================
        // SPRINT 4: INDIVIDUAL MILESTONE EVALUATION PERISTENCE MAPPINGS
        // =========================================================================

        // Configure milestone score to preserve rounding precision inside SQL scales
        builder.Property(m => m.Grade)
            .IsRequired(false)
            .HasColumnType("decimal(5,2)");

        builder.Property(m => m.EvaluationFeedback)
            .IsRequired(false)
            .HasMaxLength(4000);

        builder.Property(m => m.GradedAt)
            .IsRequired(false);

        // =========================================================================
        // SPRINT 4.5: CONVERSATION ENCAPSULATION SETTINGS
        // =========================================================================

        // Direct EF Core to route materialization maps for the Comments collection 
        // straight through its underlying private list backing field to honor strict domain invariants.
        builder.HasMany(m => m.Comments)
            .WithOne()
            .HasForeignKey(c => c.LocalMilestoneId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(LocalMilestone.Comments))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Performance Optimization Index: Speeds up parent container workspace graph hydration passes
        builder.HasIndex(m => m.ProjectInstanceId)
            .HasDatabaseName("IX_LocalMilestones_ProjectInstanceId");
    }
}
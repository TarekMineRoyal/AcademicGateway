using AcademicGateway.Domain.Common.Enums;
using AcademicGateway.Domain.ProjectInstances;
using AcademicGateway.Domain.ProjectInstances.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGateway.Infrastructure.Persistence.Configurations;

/// <summary>
/// Database configuration mappings for the <see cref="LocalMilestone"/> execution entity.
/// Establishes table constraints, precision controls for scheduling metrics, and enum text conversions.
/// </summary>
public class LocalMilestoneConfiguration : IEntityTypeConfiguration<LocalMilestone>
{
    /// <summary>
    /// Configures the relational boundaries, data field lengths, and index constraints for runtime milestones.
    /// </summary>
    /// <param name="builder">The API builder instance used to define internal database schemas.</param>
    public void Configure(EntityTypeBuilder<LocalMilestone> builder)
    {
        // Define explicit physical database table name mapping
        builder.ToTable("LocalMilestones");

        // Primary Key registration
        builder.HasKey(x => x.Id);

        // Core tracking reference to parent project aggregate root context
        builder.Property(x => x.ProjectInstanceId)
            .IsRequired();

        // Historical snapshot text variables inherited from blueprint
        builder.Property(x => x.TitleSnapshot)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.DescriptionSnapshot)
            .HasMaxLength(4000)
            .IsRequired();

        // Effort and evaluation numeric measurements require explicit scale definition in SQL
        builder.Property(x => x.ExpectedEffortInHours)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.Grade)
            .HasColumnType("decimal(5,2)")
            .IsRequired(false);

        // Polymorphic submissions and feedback commentary envelopes
        builder.Property(x => x.SubmissionPayload)
            .HasMaxLength(4000)
            .IsRequired(false);

        builder.Property(x => x.EvaluationFeedback)
            .HasMaxLength(2000)
            .IsRequired(false);

        // Student-assigned architectural dates and timeline limits
        builder.Property(x => x.ScheduledStartDate)
            .IsRequired(false);

        builder.Property(x => x.ScheduledEndDate)
            .IsRequired(false);

        // Administrative operational timestamps
        builder.Property(x => x.SubmittedAt)
            .IsRequired(false);

        builder.Property(x => x.GradedAt)
            .IsRequired(false);

        // Maps structural enums safely to text formats for cleaner database auditing logs
        builder.Property(x => x.RequiredDeliverableType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        // RELATIONSHIP BOUNDARIES
        // Configures the structural graph edge collection pointing into milestone nodes
        builder.HasMany(x => x.InboundDependencies)
            .WithOne()
            .HasForeignKey(x => x.SuccessorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
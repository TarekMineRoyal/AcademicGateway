using AcademicGateway.Domain.ProjectInstances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGateway.Infrastructure.Persistence.Configurations;

/// <summary>
/// Relational database configurations and table mapping constraints governing the persistence layout of the <see cref="LocalTask"/> entity.
/// Dictates explicit storage properties for active nested workflow tasks, execution payloads, and grading indicators.
/// </summary>
public class LocalTaskConfiguration : IEntityTypeConfiguration<LocalTask>
{
    /// <summary>
    /// Configures the relational database architecture, keys, snapshots, constraints, conversions, and optimized indexes for local runtime tasks.
    /// </summary>
    public void Configure(EntityTypeBuilder<LocalTask> builder)
    {
        // Define explicit relational database storage table mapping destination
        builder.ToTable("LocalTasks");

        // Set primary key identifier parameters
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        // Structural relationship identifier mapping
        builder.Property(t => t.LocalMilestoneId)
            .IsRequired();

        // Snapshot text definitions and length criteria constraints
        builder.Property(t => t.TitleSnapshot)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.DescriptionSnapshot)
            .IsRequired()
            .HasMaxLength(4000);

        // Operational Work Breakdown Structure (WBS) progress layout allocation
        builder.Property(t => t.Weight)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        // Enum conversions to database string tokens with structural text length constraints
        builder.Property(t => t.RequiredDeliverableType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // Optional/Nullable task execution runtime fields
        builder.Property(t => t.SubmissionPayload)
            .IsRequired(false)
            .HasMaxLength(4000);

        builder.Property(t => t.SubmittedAt)
            .IsRequired(false);

        // Task Academic Evaluation Persistence Mappings
        builder.Property(t => t.Grade)
            .IsRequired(false)
            .HasColumnType("decimal(5,2)");

        builder.Property(t => t.EvaluationFeedback)
            .IsRequired(false)
            .HasMaxLength(4000);

        builder.Property(t => t.GradedAt)
            .IsRequired(false);

        // Relational index optimization to resolve read-side flattening pipelines efficiently
        builder.HasIndex(t => t.LocalMilestoneId)
            .HasDatabaseName("IX_LocalTasks_LocalMilestoneId");
    }
}
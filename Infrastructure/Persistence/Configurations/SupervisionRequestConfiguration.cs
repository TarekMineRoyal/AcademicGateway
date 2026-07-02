using AcademicGateway.Domain.ProjectInstances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGateway.Infrastructure.Persistence.Configurations;

/// <summary>
/// Database configuration mappings for the <see cref="SupervisionRequest"/> entity.
/// Configures table schemas, keys, constraints, property conversions, and relational constraints.
/// </summary>
public class SupervisionRequestConfiguration : IEntityTypeConfiguration<SupervisionRequest>
{
    /// <summary>
    /// Configures the database schema boundaries, field rules, and foreign keys for <see cref="SupervisionRequest"/>.
    /// </summary>
    /// <param name="builder">The builder API used to define the entity configuration properties.</param>
    public void Configure(EntityTypeBuilder<SupervisionRequest> builder)
    {
        // Define explicit physical database table name mapping
        builder.ToTable("SupervisionRequests");

        // Primary Key definition
        builder.HasKey(x => x.Id);

        // Core relational foreign key tracking identifiers
        builder.Property(x => x.ProjectInstanceId)
            .IsRequired();

        builder.Property(x => x.ProfessorId)
            .IsRequired();

        // Administrative content textual fields and length constraints
        builder.Property(x => x.PitchText)
            .HasMaxLength(1500)
            .IsRequired();

        builder.Property(x => x.RejectionReason)
            .HasMaxLength(500)
            .IsRequired(false);

        // Operational timestamp parameters
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.ReviewedAt)
            .IsRequired(false);

        // Maps the matching SupervisionRequestStatus state machine enum to a clean readable string in SQL storage rows
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        // RELATIONSHIP MAPPING
        builder.HasOne(x => x.ProjectInstance)
            .WithMany(x => x.SupervisionRequests)
            .HasForeignKey(x => x.ProjectInstanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
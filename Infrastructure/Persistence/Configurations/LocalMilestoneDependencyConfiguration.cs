using AcademicGateway.Domain.Common.Enums;
using AcademicGateway.Domain.ProjectInstances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGateway.Infrastructure.Persistence.Configurations;

/// <summary>
/// Database configuration mappings for the <see cref="LocalMilestoneDependency"/> graph relationship entity.
/// Establishes composite operational keys and protects the database from duplicate cascade loops.
/// </summary>
public class LocalMilestoneDependencyConfiguration : IEntityTypeConfiguration<LocalMilestoneDependency>
{
    /// <summary>
    /// Maps the structural join criteria and foreign keys representing the directed execution graph edges.
    /// </summary>
    /// <param name="builder">The API builder instance used to define relational constraints.</param>
    public void Configure(EntityTypeBuilder<LocalMilestoneDependency> builder)
    {
        // Define explicit physical database table name mapping
        builder.ToTable("LocalMilestoneDependencies");

        // Establishes a composite primary key to block duplicate relationship assignments across matching nodes
        builder.HasKey(x => new { x.PredecessorId, x.SuccessorId });

        // Map behavior constraint rules to an auditable text column format
        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        // RELATIONSHIP MAPS & ENGINE SAFETY CORRECTIONS
        // Bind the predecessor lookup chain. We enforce Restrict here to completely bypass multiple cascade path errors.
        builder.HasOne<LocalMilestone>()
            .WithMany()
            .HasForeignKey(x => x.PredecessorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
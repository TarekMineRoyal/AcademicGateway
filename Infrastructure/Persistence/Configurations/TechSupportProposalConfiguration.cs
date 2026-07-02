using AcademicGateway.Domain.ProjectInstances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGateway.Infrastructure.Persistence.Configurations;

/// <summary>
/// Database configuration mappings for the <see cref="TechSupportProposal"/> entity within the tarekmineroyal/academicgateway project.
/// Configures table schemas, keys, constraints, property conversions, and relational constraints.
/// </summary>
public class TechSupportProposalConfiguration : IEntityTypeConfiguration<TechSupportProposal>
{
    /// <summary>
    /// Configures the database schema boundaries, field rules, and foreign keys for <see cref="TechSupportProposal"/>.
    /// </summary>
    /// <param name="builder">The builder API used to define the entity configuration properties.</param>
    public void Configure(EntityTypeBuilder<TechSupportProposal> builder)
    {
        // Define explicit physical database table name mapping
        builder.ToTable("TechSupportProposals");

        // Primary Key definition
        builder.HasKey(x => x.Id);

        // Core relational foreign key tracking identifiers
        builder.Property(x => x.ProjectInstanceId)
            .IsRequired();

        // Operational timestamp parameters
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Maps the matching TechSupportProposalStatus state machine enum to a clean readable string in SQL storage rows
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        // RELATIONSHIP MAPPING
        builder.HasOne(x => x.ProjectInstance)
            .WithMany(x => x.TechSupportProposals)
            .HasForeignKey(x => x.ProjectInstanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
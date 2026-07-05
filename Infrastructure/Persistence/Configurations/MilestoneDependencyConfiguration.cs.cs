using AcademicGateway.Domain.ProjectTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGateway.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mappings and constraints governing the blueprint template milestone dependency matrix.
/// </summary>
public class MilestoneDependencyConfiguration : IEntityTypeConfiguration<MilestoneDependency>
{
    /// <summary>
    /// Configures the relational table layout and composite key tracking for blueprint milestones.
    /// </summary>
    public void Configure(EntityTypeBuilder<MilestoneDependency> builder)
    {
        // Map to its independent template blueprint table destination
        builder.ToTable("TemplateMilestoneDependencies");

        // Establish the composite tracking key using actual CLR domain properties
        builder.HasKey(d => new { d.PredecessorId, d.SuccessorId });

        builder.Property(d => d.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
    }
}
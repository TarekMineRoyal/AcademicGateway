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

        // Establish the composite tracking key for the template graph.
        // Note: If your domain model property uses "TemplateMilestoneId" instead of 
        // "ProjectTemplateMilestoneId", update the string literal below to match your domain property.
        builder.HasKey("ProjectTemplateMilestoneId", "PredecessorId");

        builder.Property(d => d.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
    }
}
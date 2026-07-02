using AcademicGateway.Domain.ProjectInstances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGateway.Infrastructure.Persistence.Configurations;

/// <summary>
/// Database configuration mappings for the <see cref="ProjectInstance"/> aggregate root boundary.
/// </summary>
public class ProjectInstanceConfiguration : IEntityTypeConfiguration<ProjectInstance>
{
    public void Configure(EntityTypeBuilder<ProjectInstance> builder)
    {
        // Define explicit physical database table name
        builder.ToTable("ProjectInstances");

        // Primary Key definition
        builder.HasKey(x => x.Id);

        // Core relational tracking identifiers
        builder.Property(x => x.StudentId)
            .IsRequired();

        builder.Property(x => x.ProviderId)
            .IsRequired();

        builder.Property(x => x.TemplateId)
            .IsRequired();

        // Calendar deadlines and limits
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.EndDate)
            .IsRequired();

        // Map the Status enum code strictly to a clean readable string description in SQL rows
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
    }
}
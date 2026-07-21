using AcademicGateway.Infrastructure.Identity;
using AcademicGateway.Domain.Professors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGateway.Infrastructure.Persistence.Configurations;

/// <summary>
/// Relational database configuration mapping for the <see cref="Professor"/> aggregate root.
/// </summary>
public class ProfessorConfiguration : IEntityTypeConfiguration<Professor>
{
    public void Configure(EntityTypeBuilder<Professor> builder)
    {
        builder.ToTable("Professors");

        // Identity Mapping (The Id maps 1:1 with the ApplicationUser Id)
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        // Establish 1:1 foreign key binding directly to the Identity Framework user account record
        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Professor>(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Structural Faculty Attributes
        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Department)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Rank)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.AboutMe)
            .IsRequired(false)
            .HasMaxLength(2000);

        builder.Property(x => x.MaxSupervisionCapacity)
            .IsRequired();

        builder.Property(x => x.CurrentProjectCount)
            .IsRequired();

        // Encapsulated Research Interests Join Collection Mapping
        // Accesses the internal backing field directly, matching the refactored domain property name
        builder.HasMany(x => x.ResearchInterests)
            .WithOne(x => x.Professor)
            .HasForeignKey(x => x.ProfessorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Professor.ResearchInterests))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

/// <summary>
/// Relational database configuration mapping for the many-to-many join entity <see cref="ProfessorResearchInterest"/>.
/// </summary>
public class ProfessorResearchInterestConfiguration : IEntityTypeConfiguration<ProfessorResearchInterest>
{
    public void Configure(EntityTypeBuilder<ProfessorResearchInterest> builder)
    {
        builder.ToTable("ProfessorResearchInterests");

        // Set composite primary key for intersection lookups
        builder.HasKey(x => new { x.ProfessorId, x.ResearchInterestId });

        // Link back to parent aggregate root safely using its true domain collection
        builder.HasOne(x => x.Professor)
            .WithMany(p => p.ResearchInterests)
            .HasForeignKey(x => x.ProfessorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Link over to the scientific lookup reference using its true domain collection
        builder.HasOne(x => x.ResearchInterest)
            .WithMany(r => r.ProfessorLinks)
            .HasForeignKey(x => x.ResearchInterestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Relational database configuration mapping for the master global lookup entity <see cref="ResearchInterest"/>.
/// </summary>
public class ResearchInterestConfiguration : IEntityTypeConfiguration<ResearchInterest>
{
    public void Configure(EntityTypeBuilder<ResearchInterest> builder)
    {
        builder.ToTable("ResearchInterests");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Area)
            .IsRequired()
            .HasMaxLength(200);

        // Back-reference collection routing through internal field access
        builder.HasMany(x => x.ProfessorLinks)
            .WithOne(x => x.ResearchInterest)
            .HasForeignKey(x => x.ResearchInterestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(ResearchInterest.ProfessorLinks))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
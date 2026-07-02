using AcademicGateway.Domain.Skills;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGateway.Infrastructure.Persistence.Configurations;

/// <summary>
/// Relational database configuration mapping for the <see cref="Skill"/> aggregate lookup root.
/// Enforces uniqueness constraints, string lengths, and backing field mapping rules.
/// </summary>
public class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ToTable("Skills");

        // Identity Mapping
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        // Core Capability Attributes
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        // Configure a unique index to ensure no duplicate skill strings exist in the lookup dictionary
        builder.HasIndex(x => x.Name)
            .IsUnique();

        // Encapsulated Student-to-Skill Join Collection Mapping
        // Configures field-level access to the read-only backing collection property
        builder.HasMany(x => x.StudentSkills)
            .WithOne(ss => ss.Skill)
            .HasForeignKey(ss => ss.SkillId)
            .OnDelete(DeleteBehavior.Restrict); // Protects global lookup values from cascading profile drops

        builder.Metadata
            .FindNavigation(nameof(Skill.StudentSkills))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
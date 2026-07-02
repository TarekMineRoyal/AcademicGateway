using AcademicGateway.Domain.Curriculum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGateway.Infrastructure.Persistence.Configurations;

/// <summary>
/// Relational database configuration mapping for the <see cref="Major"/> aggregate root.
/// Configures text attributes and configures explicit field-level accessors for encapsulated child track fields.
/// </summary>
public class MajorConfiguration : IEntityTypeConfiguration<Major>
{
    public void Configure(EntityTypeBuilder<Major> builder)
    {
        builder.ToTable("Majors");

        // Identity Mapping
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        // Core Academic Attributes
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Encapsulated Child Specialty Paths Collection Mapping
        builder.HasMany(x => x.Specialties)
            .WithOne(x => x.Major)
            .HasForeignKey(x => x.MajorId)
            .OnDelete(DeleteBehavior.Cascade); // Deleting a Major cascades down to wipe its sub-specialties cleanly

        builder.Metadata
            .FindNavigation(nameof(Major.Specialties))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Inform EF Core about the encapsulated StudentMajors tracking array backing field
        builder.HasMany(x => x.StudentMajors)
            .WithOne(x => x.Major)
            .HasForeignKey(x => x.MajorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Metadata
            .FindNavigation(nameof(Major.StudentMajors))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

/// <summary>
/// Relational database configuration mapping for the dependent child <see cref="Specialty"/> entity.
/// </summary>
public class SpecialtyConfiguration : IEntityTypeConfiguration<Specialty>
{
    public void Configure(EntityTypeBuilder<Specialty> builder)
    {
        builder.ToTable("Specialties");

        // Identity Mapping
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        // Core Concentration Attributes
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.MajorId)
            .IsRequired();

        // Parent Relationship Linkage
        builder.HasOne(x => x.Major)
            .WithMany(m => m.Specialties)
            .HasForeignKey(x => x.MajorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Inform EF Core about the encapsulated StudentSpecialties tracking array backing field
        builder.HasMany(x => x.StudentSpecialties)
            .WithOne(x => x.Specialty)
            .HasForeignKey(x => x.SpecialtyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Metadata
            .FindNavigation(nameof(Specialty.StudentSpecialties))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
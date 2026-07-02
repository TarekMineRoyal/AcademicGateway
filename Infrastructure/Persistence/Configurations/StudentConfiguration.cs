using AcademicGateway.Infrastructure.Identity;
using AcademicGateway.Domain.Curriculum;
using AcademicGateway.Domain.Skills;
using AcademicGateway.Domain.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGateway.Infrastructure.Persistence.Configurations;

/// <summary>
/// Relational database configuration mapping for the <see cref="Student"/> aggregate root.
/// Enforces domain invariants, property nullability parameters, and 1:1 Identity mappings.
/// </summary>
public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        // Identity Mapping (The Id maps 1:1 with the ApplicationUser Id surrogate)
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        // Establish 1:1 foreign key binding directly to the Identity framework's user tables
        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Student>(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Core Domain Profile Attributes
        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(200);

        // Configured correctly as an optional property matching the nullable int? domain descriptor
        builder.Property(x => x.GraduationYear)
            .IsRequired(false);

        // Encapsulated Majors Join Collection Mapping
        // Accesses the internal backing fields directly, bypassing the read-only public properties
        builder.HasMany(x => x.StudentMajors)
            .WithOne(sm => sm.Student)
            .HasForeignKey(sm => sm.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Student.StudentMajors))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Encapsulated Specialties Join Collection Mapping
        builder.HasMany(x => x.StudentSpecialties)
            .WithOne(ss => ss.Student)
            .HasForeignKey(ss => ss.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Student.StudentSpecialties))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Encapsulated Skills Join Collection Mapping
        builder.HasMany(x => x.StudentSkills)
            .WithOne(ss => ss.Student)
            .HasForeignKey(ss => ss.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Student.StudentSkills))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

/// <summary>
/// Relational database configuration mapping for the many-to-many join entity <see cref="StudentMajor"/>.
/// </summary>
public class StudentMajorConfiguration : IEntityTypeConfiguration<StudentMajor>
{
    public void Configure(EntityTypeBuilder<StudentMajor> builder)
    {
        builder.ToTable("StudentMajors");
        builder.HasKey(x => new { x.StudentId, x.MajorId });

        builder.HasOne(x => x.Student)
            .WithMany(s => s.StudentMajors)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Major)
            .WithMany(m => m.StudentMajors)
            .HasForeignKey(x => x.MajorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Relational database configuration mapping for the many-to-many join entity <see cref="StudentSpecialty"/>.
/// </summary>
public class StudentSpecialtyConfiguration : IEntityTypeConfiguration<StudentSpecialty>
{
    public void Configure(EntityTypeBuilder<StudentSpecialty> builder)
    {
        builder.ToTable("StudentSpecialties");
        builder.HasKey(x => new { x.StudentId, x.SpecialtyId });

        builder.HasOne(x => x.Student)
            .WithMany(s => s.StudentSpecialties)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Specialty)
            .WithMany(s => s.StudentSpecialties)
            .HasForeignKey(x => x.SpecialtyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Relational database configuration mapping for the many-to-many join entity <see cref="StudentSkill"/>.
/// </summary>
public class StudentSkillConfiguration : IEntityTypeConfiguration<StudentSkill>
{
    public void Configure(EntityTypeBuilder<StudentSkill> builder)
    {
        builder.ToTable("StudentSkills");
        builder.HasKey(x => new { x.StudentId, x.SkillId });

        builder.HasOne(x => x.Student)
            .WithMany(s => s.StudentSkills)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Skill)
            .WithMany(s => s.StudentSkills)
            .HasForeignKey(x => x.SkillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
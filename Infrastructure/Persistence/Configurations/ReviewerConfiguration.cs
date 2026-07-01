using AcademicGateway.Infrastructure.Identity;
using Domain.Providers;
using Domain.SystemStaff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGateway.Infrastructure.Persistence.Configurations;

/// <summary>
/// Relational database configuration mapping for the <see cref="Reviewer"/> profile entity.
/// Configures text lengths, primary user key bindings, and explicit field-level accessors for historical audit fields.
/// </summary>
public class ReviewerConfiguration : IEntityTypeConfiguration<Reviewer>
{
    public void Configure(EntityTypeBuilder<Reviewer> builder)
    {
        builder.ToTable("Reviewers");

        // Identity Mapping (The Id maps 1:1 with the ApplicationUser Id surrogate)
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        // Establish 1:1 foreign key binding directly to the Identity Framework user account record
        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Reviewer>(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Structural Evaluator Attributes
        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(200);

        // Encapsulated Historical Audit Collection Mapping
        // Maps the historical audit log array straight to its corresponding backing field
        builder.HasMany(x => x.ReviewedApplications)
            .WithOne(pa => pa.ReviewedBy)
            .HasForeignKey(pa => pa.ReviewedById)
            .OnDelete(DeleteBehavior.SetNull); // Maintains historical application audits even if a reviewer profile is dropped

        builder.Metadata
            .FindNavigation(nameof(Reviewer.ReviewedApplications))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
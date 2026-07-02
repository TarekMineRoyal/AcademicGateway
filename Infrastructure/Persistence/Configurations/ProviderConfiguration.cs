using AcademicGateway.Infrastructure.Identity;
using AcademicGateway.Domain.ProjectTemplates;
using AcademicGateway.Domain.Providers;
using AcademicGateway.Domain.Providers.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace AcademicGateway.Infrastructure.Persistence.Configurations;

/// <summary>
/// Relational database configuration mapping for the <see cref="Provider"/> aggregate root.
/// Enforces domain invariants, property lengths, and 1:1 Identity mappings.
/// </summary>
public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("Providers");

        // Identity Mapping (The Id maps 1:1 with the ApplicationUser Id)
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        // Establish 1:1 foreign key binding directly to the Identity Framework user account record
        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Provider>(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Core Company Metadata
        builder.Property(x => x.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.CompanyDescription)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.WebsiteUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.IsVerified)
            .IsRequired()
            .HasDefaultValue(false);

        // Encapsulated Project Blueprints Backing Collection Mapping
        // Configures field-level access to the read-only backing collection field
        builder.HasMany(x => x.ProjectTemplates)
            .WithOne(pt => pt.Provider)
            .HasForeignKey(pt => pt.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Provider.ProjectTemplates))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

/// <summary>
/// Relational database configuration mapping for the stateful <see cref="ProviderApplication"/> onboarding workflow.
/// </summary>
public class ProviderApplicationConfiguration : IEntityTypeConfiguration<ProviderApplication>
{
    public void Configure(EntityTypeBuilder<ProviderApplication> builder)
    {
        builder.ToTable("ProviderApplications");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.CompanyDetails)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.VerificationDocumentsUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.RejectionReason)
            .IsRequired(false)
            .HasMaxLength(1000);

        // State Machine Enum Conversion Mapping
        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (ProviderApplicationStatus)Enum.Parse(typeof(ProviderApplicationStatus), v))
            .HasMaxLength(50);

        // Audit Lifecycle Timestamps
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.ReviewedAt)
            .IsRequired(false);

        // Formally configure relationships using defined entity property boundaries
        builder.HasOne(x => x.Provider)
            .WithMany()
            .HasForeignKey(x => x.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ReviewedBy)
            .WithMany(r => r.ReviewedApplications)
            .HasForeignKey(x => x.ReviewedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

/// <summary>
/// Relational database configuration mapping for the <see cref="TechSupportAccount"/> profile entity.
/// </summary>
public class TechSupportAccountConfiguration : IEntityTypeConfiguration<TechSupportAccount>
{
    public void Configure(EntityTypeBuilder<TechSupportAccount> builder)
    {
        builder.ToTable("TechSupportAccounts");

        // Identity Mapping (The Id maps 1:1 with the ApplicationUser Id)
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        // Establish 1:1 foreign key binding directly to the Identity Framework user account record
        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<TechSupportAccount>(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Core Support Operational Fields
        builder.Property(x => x.StaffNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.SupportTier)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
    }
}
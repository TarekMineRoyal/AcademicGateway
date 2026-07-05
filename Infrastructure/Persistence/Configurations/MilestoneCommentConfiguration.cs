using AcademicGateway.Domain.ProjectInstances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGateway.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mappings and constraints governing the persistence storage layout of the <see cref="MilestoneComment"/> child entity.
/// Establishes the safe database tracking channel parameters for cross-role collaboration history logs.
/// </summary>
public class MilestoneCommentConfiguration : IEntityTypeConfiguration<MilestoneComment>
{
    /// <summary>
    /// Configures the relational database table architecture, key mappings, and indexing strategies for milestone comments.
    /// </summary>
    public void Configure(EntityTypeBuilder<MilestoneComment> builder)
    {
        // Define explicit storage table mapping destination
        builder.ToTable("MilestoneComments");

        // Set primary key identifier parameters
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        // Enforce basic column string sanitation limits and indexes
        builder.Property(c => c.AuthorId)
            .IsRequired();

        builder.Property(c => c.AuthorIdentitySnapshot)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        // Establish strict relationship mappings pointing back to the parent LocalMilestone line.
        // Cascades deletions natively so that if a local milestone snapshot graph is purged, its discussion logs clear out automatically.
        builder.HasOne<LocalMilestone>()
            .WithMany(m => m.Comments)
            .HasForeignKey(c => c.LocalMilestoneId)
            .OnDelete(DeleteBehavior.Cascade);

        // Performance Optimization Index: Accelerates conversational thread rendering lookups 
        // when loading the chronological timeline feed for a specific milestone block.
        builder.HasIndex(c => c.LocalMilestoneId)
            .HasDatabaseName("IX_MilestoneComments_LocalMilestoneId");
    }
}
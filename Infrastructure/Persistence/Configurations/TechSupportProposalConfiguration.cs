using AcademicGateway.Domain.ProjectInstances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGateway.Infrastructure.Persistence.Configurations;

public class TechSupportProposalConfiguration : IEntityTypeConfiguration<TechSupportProposal>
{
    public void Configure(EntityTypeBuilder<TechSupportProposal> builder)
    {
        builder.ToTable("TechSupportProposals");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProjectInstanceId)
            .IsRequired();

        builder.Property(x => x.TechSupportAccountId).IsRequired();
        builder.Property(x => x.RejectionReason).HasMaxLength(500).IsRequired(false);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        // Bind the relationship to the explicitly mapped property
        builder.HasOne<ProjectInstance>()
            .WithMany(x => x.TechSupportProposals)
            .HasForeignKey(x => x.ProjectInstanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
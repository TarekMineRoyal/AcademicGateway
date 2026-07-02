using AcademicGateway.Domain.ProjectInstances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGateway.Infrastructure.Persistence.Configurations;

public class SupervisionRequestConfiguration : IEntityTypeConfiguration<SupervisionRequest>
{
    public void Configure(EntityTypeBuilder<SupervisionRequest> builder)
    {
        builder.ToTable("SupervisionRequests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProjectInstanceId)
            .IsRequired();

        builder.Property(x => x.ProfessorId).IsRequired();
        builder.Property(x => x.PitchText).HasMaxLength(1500).IsRequired();
        builder.Property(x => x.RejectionReason).HasMaxLength(500).IsRequired(false);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ReviewedAt).IsRequired(false);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        // Bind the relationship to the explicitly mapped property
        builder.HasOne<ProjectInstance>()
            .WithMany(x => x.SupervisionRequests)
            .HasForeignKey(x => x.ProjectInstanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
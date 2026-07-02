using AcademicGateway.Domain.ProjectInstances;
using AcademicGateway.Domain.Skills;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGateway.Infrastructure.Persistence.Configurations;

public class ProjectInstanceSkillConfiguration : IEntityTypeConfiguration<ProjectInstanceSkill>
{
    public void Configure(EntityTypeBuilder<ProjectInstanceSkill> builder)
    {
        builder.ToTable("ProjectInstanceSkills");

        // Explicitly map the primitive Guid properties as columns
        builder.Property(x => x.ProjectInstanceId).IsRequired();
        builder.Property(x => x.SkillId).IsRequired();

        // Define the composite primary key using the mapped columns
        builder.HasKey(x => new { x.ProjectInstanceId, x.SkillId });

        // Bind the relationships cleanly
        builder.HasOne<ProjectInstance>()
            .WithMany()
            .HasForeignKey(x => x.ProjectInstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Skill>()
            .WithMany()
            .HasForeignKey(x => x.SkillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
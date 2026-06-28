using AcademicGateway.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // Existing DbSets
    DbSet<Student> Students { get; }
    DbSet<Provider> Providers { get; }
    DbSet<Professor> Professors { get; }
    DbSet<Skill> Skills { get; }
    DbSet<StudentSkill> StudentSkills { get; }
    DbSet<Major> Majors { get; }
    DbSet<Specialty> Specialties { get; }
    DbSet<StudentMajor> StudentMajors { get; }
    DbSet<StudentSpecialty> StudentSpecialties { get; }
    DbSet<Reviewer> Reviewers { get; }
    DbSet<ProviderApplication> ProviderApplications { get; }
    DbSet<ProjectTemplate> ProjectTemplates { get; }
    DbSet<ProjectTemplateSkill> ProjectTemplateSkills { get; }
    DbSet<TechSupportAccount> TechSupportAccounts { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
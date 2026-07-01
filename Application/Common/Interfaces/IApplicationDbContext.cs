using Domain.Curriculum;
using Domain.Professors;
using Domain.ProjectTemplates;
using Domain.Providers;
using Domain.Skills;
using Domain.Students;
using Domain.SystemStaff;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Common.Interfaces;

/// <summary>
/// Defines the centralized unit-of-work behavioral contract and relational data access layer boundaries.
/// Decouples the application layer execution flows from the concrete infrastructure Entity Framework Core persistence engine.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Gets the database tracking context set for the <see cref="Student"/> aggregate root profiles.
    /// </summary>
    DbSet<Student> Students { get; }

    /// <summary>
    /// Gets the database tracking context set for the corporate <see cref="Provider"/> aggregate root profiles.
    /// </summary>
    DbSet<Provider> Providers { get; }

    /// <summary>
    /// Gets the database tracking context set for the <see cref="Professor"/> aggregate root profiles.
    /// </summary>
    DbSet<Professor> Professors { get; }

    /// <summary>
    /// Gets the database tracking context set for the master dictionary of technical <see cref="Skill"/> entries.
    /// </summary>
    DbSet<Skill> Skills { get; }

    /// <summary>
    /// Gets the database tracking context set for the <see cref="StudentSkill"/> many-to-many join intersection records.
    /// </summary>
    DbSet<StudentSkill> StudentSkills { get; }

    /// <summary>
    /// Gets the database tracking context set for the master directory of institutional academic <see cref="Major"/> paths.
    /// </summary>
    DbSet<Major> Majors { get; }

    /// <summary>
    /// Gets the database tracking context set for specialized academic sub-tracks mapped under <see cref="Specialty"/> models.
    /// </summary>
    DbSet<Specialty> Specialties { get; }

    /// <summary>
    /// Gets the database tracking context set for the <see cref="StudentMajor"/> many-to-many join intersection records.
    /// </summary>
    DbSet<StudentMajor> StudentMajors { get; }

    /// <summary>
    /// Gets the database tracking context set for the <see cref="StudentSpecialty"/> many-to-many join intersection records.
    /// </summary>
    DbSet<StudentSpecialty> StudentSpecialties { get; }

    /// <summary>
    /// Gets the database tracking context set for internal administrative quality compliance <see cref="Reviewer"/> profiles.
    /// </summary>
    DbSet<Reviewer> Reviewers { get; }

    /// <summary>
    /// Gets the database tracking context set for monitoring state-machine <see cref="ProviderApplication"/> onboarding submissions.
    /// </summary>
    DbSet<ProviderApplication> ProviderApplications { get; }

    /// <summary>
    /// Gets the database tracking context set for structural industry-backed <see cref="ProjectTemplate"/> schemas.
    /// </summary>
    DbSet<ProjectTemplate> ProjectTemplates { get; }

    /// <summary>
    /// Gets the database tracking context set for the <see cref="ProjectTemplateSkill"/> many-to-many join intersection records.
    /// </summary>
    DbSet<ProjectTemplateSkill> ProjectTemplateSkills { get; }

    /// <summary>
    /// Gets the database tracking context set for technical support and administrative platform maintenance <see cref="TechSupportAccount"/> profiles.
    /// </summary>
    DbSet<TechSupportAccount> TechSupportAccounts { get; }

    /// <summary>
    /// Atomically flushes all tracked collection alterations, structural updates, and outstanding aggregate domain state mutations down to the relational persistence layer.
    /// </summary>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>A task tracking the asynchronous operation execution, returning the aggregate count of database state modifications committed.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
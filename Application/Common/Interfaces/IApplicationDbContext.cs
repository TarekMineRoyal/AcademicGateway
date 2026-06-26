using AcademicGateway.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Student> Students { get; }
    DbSet<Provider> Providers { get; }
    DbSet<Professor> Professors { get; }
    DbSet<Skill> Skills { get; }
    DbSet<StudentSkill> StudentSkills { get; }
    DbSet<Major> Majors { get; }
    DbSet<Specialty> Specialties { get; }
    DbSet<StudentMajor> StudentMajors { get; }
    DbSet<StudentSpecialty> StudentSpecialties { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
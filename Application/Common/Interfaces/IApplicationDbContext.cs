using AcademicGateway.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace AcademicGateway.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Student> Students { get; }
    DbSet<Provider> Providers { get; }
    DbSet<Professor> Professors { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
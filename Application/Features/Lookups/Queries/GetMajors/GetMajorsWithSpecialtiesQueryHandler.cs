using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Lookups.Queries.GetMajors;

/// <summary>
/// Handles the execution of the <see cref="GetMajorsWithSpecialtiesQuery"/> lookup request.
/// Leverages high-performance, untracked relational database projection to fetch the curriculum hierarchy.
/// </summary>
public class GetMajorsWithSpecialtiesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetMajorsWithSpecialtiesQuery, IReadOnlyCollection<MajorDto>>
{
    /// <summary>
    /// Processes the query by mapping database-level entities straight into clean, read-only lookups.
    /// </summary>
    /// <param name="request">The incoming parameterless lookup trigger execution payload.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>An immutable read-only sequence containing all configured academic major and sub-specialty structures.</returns>
    public async Task<IReadOnlyCollection<MajorDto>> Handle(GetMajorsWithSpecialtiesQuery request, CancellationToken cancellationToken)
    {
        return await context.Majors
            .AsNoTracking()
            .Select(major => new MajorDto
            {
                Id = major.Id,
                Name = major.Name,
                Specialties = major.Specialties
                    .Select(specialty => new SpecialtyDto
                    {
                        Id = specialty.Id,
                        Name = specialty.Name
                    })
                    .ToList() // EF Core translates this smoothly to materialise a concrete collection subquery projection
            })
            .ToListAsync(cancellationToken);
    }
}
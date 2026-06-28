using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Lookups.Queries.GetMajors;

public class GetMajorsWithSpecialtiesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetMajorsWithSpecialtiesQuery, List<MajorDto>>
{
    public async Task<List<MajorDto>> Handle(GetMajorsWithSpecialtiesQuery request, CancellationToken cancellationToken)
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
                    .ToList()
            })
            .ToListAsync(cancellationToken);
    }
}
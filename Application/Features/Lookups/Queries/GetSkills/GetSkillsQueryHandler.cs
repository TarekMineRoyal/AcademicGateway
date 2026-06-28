using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Lookups.Queries.GetSkills;

public class GetSkillsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSkillsQuery, List<SkillDto>>
{
    public async Task<List<SkillDto>> Handle(GetSkillsQuery request, CancellationToken cancellationToken)
    {
        return await context.Skills
            .AsNoTracking()
            .Select(skill => new SkillDto
            {
                Id = skill.Id,
                Name = skill.Name
            })
            .ToListAsync(cancellationToken);
    }
}
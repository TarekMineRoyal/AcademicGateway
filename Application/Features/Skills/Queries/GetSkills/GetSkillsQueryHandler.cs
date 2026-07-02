using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Skills.Queries.GetSkills;

/// <summary>
/// Handles the execution of the <see cref="GetSkillsQuery"/> lookup request.
/// Leverages a high-performance, untracked relational database query projection to retrieve global skills data.
/// </summary>
public class GetSkillsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSkillsQuery, IReadOnlyCollection<SkillDto>>
{
    /// <summary>
    /// Processes the query by mapping database-level skills directly into clean, read-only data transfer contracts.
    /// </summary>
    /// <param name="request">The incoming parameterless skills lookup trigger execution payload.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>An immutable read-only sequence containing all configured technical and professional skill assets.</returns>
    public async Task<IReadOnlyCollection<SkillDto>> Handle(GetSkillsQuery request, CancellationToken cancellationToken)
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
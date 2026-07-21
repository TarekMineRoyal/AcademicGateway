using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Skills.Queries.GetUserSkills;

/// <summary>
/// Handles the execution of the <see cref="GetUserSkillsQuery"/> user-specific profile lookup request[cite: 4].
/// Leverages a high-performance, untracked relational database database query projection over the student skills intersection matrix[cite: 4].
/// </summary>
public class GetUserSkillsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetUserSkillsQuery, IReadOnlyCollection<UserSkillDto>>
{
    /// <summary>
    /// Processes the query by filtering relational user-to-skill join maps and projecting directly into read-only data transfer contracts[cite: 4].
    /// </summary>
    /// <param name="request">The incoming query execution payload containing the verified target user identification constraint.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled[cite: 4].</param>
    /// <returns>An immutable read-only sequence containing all technical and professional capability assets mapped specifically onto the target identity[cite: 4].</returns>
    public async Task<IReadOnlyCollection<UserSkillDto>> Handle(GetUserSkillsQuery request, CancellationToken cancellationToken)
    {
        return await context.StudentSkills
            .AsNoTracking()
            .Where(studentSkill => studentSkill.StudentId == request.UserId)
            .Select(studentSkill => new UserSkillDto
            {
                Id = studentSkill.SkillId,
                Name = studentSkill.Skill.Name
            })
            .ToListAsync(cancellationToken);
    }
}
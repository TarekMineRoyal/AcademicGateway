using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.ProjectTemplates.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Queries.GetApprovedTemplates;

/// <summary>
/// Handles the execution of the <see cref="GetApprovedTemplatesQuery"/> request.
/// Leverages optimized, untracked relational database projections to discover public placement blueprints.
/// </summary>
public class GetApprovedTemplatesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetApprovedTemplatesQuery, IReadOnlyCollection<ApprovedTemplateDto>>
{
    /// <summary>
    /// Processes the template lookup query, applying dynamic filters and mapping results directly into read-only data contracts.
    /// </summary>
    /// <param name="request">The incoming command container tracking optional evaluation criteria (such as specialized SkillId).</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>An immutable read-only sequence containing all matched and approved project template views.</returns>
    public async Task<IReadOnlyCollection<ApprovedTemplateDto>> Handle(GetApprovedTemplatesQuery request, CancellationToken cancellationToken)
    {
        // 1. Build an IQueryable base that strictly targets Approved listings.
        // Performance Optimization: Explicit .Include and .ThenInclude statements are completely removed.
        // In CQRS read paths, a direct LINQ .Select projection automatically commands EF Core to generate 
        // the optimal inner/outer SQL JOIN queries, avoiding excessive eager loading tracking overhead.
        var query = context.ProjectTemplates
            .AsNoTracking()
            .Where(t => t.Status == ProjectTemplateStatus.Approved);

        // 2. Conditionally apply join-table skill requirements filter if supplied
        if (request.SkillId.HasValue)
        {
            // Fixed: Updated navigation query to point to 'ProjectTemplateSkills' to match the rich entity property name
            query = query.Where(t => t.ProjectTemplateSkills.Any(pts => pts.SkillId == request.SkillId.Value));
        }

        // 3. Project the filtered relational database query records directly into immutable DTO payloads
        return await query
            .Select(t => new ApprovedTemplateDto
            {
                Id = t.Id,
                ProviderId = t.ProviderId,

                // Fixed: Substituted placeholder reference to wrap your real CompanyName property
                ProviderCompanyName = t.Provider != null ? t.Provider.CompanyName : "Unknown Provider",

                Title = t.Title,
                Description = t.Description,

                // Fixed: Aligned sub-collection selection with 'ProjectTemplateSkills' entity definition
                Skills = t.ProjectTemplateSkills.Select(pts => new TemplateSkillDto
                {
                    Id = pts.SkillId,
                    Name = pts.Skill != null ? pts.Skill.Name : "Unknown Skill"
                }).ToList()
            })
            .ToListAsync(cancellationToken);
    }
}
using AcademicGateway.Application.Common.Extensions;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Common.Models;
using AcademicGateway.Domain.ProjectTemplates.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Queries.GetApprovedTemplates;

/// <summary>
/// Handles the execution of the <see cref="GetApprovedTemplatesQuery"/> request.
/// Leverages optimized, untracked relational database projections to discover public placement blueprints.
/// </summary>
public class GetApprovedTemplatesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetApprovedTemplatesQuery, PaginatedResult<ApprovedTemplateDto>>
{
    /// <summary>
    /// Processes the template lookup query, applying dynamic filters and mapping results directly into read-only data contracts with pagination.
    /// </summary>
    /// <param name="request">The incoming command container tracking optional evaluation criteria (such as specialized SkillId) and pagination options.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>A paginated result containing matched and approved project template views.</returns>
    public async Task<PaginatedResult<ApprovedTemplateDto>> Handle(GetApprovedTemplatesQuery request, CancellationToken cancellationToken)
    {
        // 1. Build an IQueryable base that strictly targets Approved listings.
        // Performance Optimization: Direct LINQ projections automatically command EF Core to generate 
        // the optimal inner/outer SQL JOIN queries, avoiding eager loading tracking overhead.
        var query = context.ProjectTemplates
            .AsNoTracking()
            .Where(t => t.Status == ProjectTemplateStatus.Approved);

        // 2. Conditionally apply join-table skill requirements filter if supplied
        if (request.SkillId.HasValue)
        {
            query = query.Where(t => t.ProjectTemplateSkills.Any(pts => pts.SkillId == request.SkillId.Value));
        }

        // 3. Project the filtered relational database query records directly into immutable DTO payloads.
        // Order by CreatedAt descending to present newly approved templates first.
        var projectedQuery = query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new ApprovedTemplateDto
            {
                Id = t.Id,
                ProviderId = t.ProviderId,

                ProviderCompanyName = t.Provider != null ? t.Provider.CompanyName : "Unknown Provider",

                Title = t.Title,
                Description = t.Description,

                MajorId = t.MajorId,
                SpecialtyId = t.SpecialtyId,

                MajorName = t.MajorId.HasValue
                    ? context.Majors.Where(m => m.Id == t.MajorId.Value).Select(m => m.Name).FirstOrDefault()
                    : null,

                SpecialtyName = t.SpecialtyId.HasValue
                    ? context.Specialties.Where(s => s.Id == t.SpecialtyId.Value).Select(s => s.Name).FirstOrDefault()
                    : null,

                Skills = t.ProjectTemplateSkills.Select(pts => new TemplateSkillDto
                {
                    Id = pts.SkillId,
                    Name = pts.Skill != null ? pts.Skill.Name : "Unknown Skill"
                }).ToList()
            });

        return await projectedQuery.ToPaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
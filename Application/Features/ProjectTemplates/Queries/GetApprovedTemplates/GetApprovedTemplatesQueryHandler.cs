using AcademicGateway.Application.Common.Interfaces;
using Domain.ProjectTemplates.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.ProjectTemplates.Queries.GetApprovedTemplates;

public class GetApprovedTemplatesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetApprovedTemplatesQuery, List<ApprovedTemplateDto>>
{
    public async Task<List<ApprovedTemplateDto>> Handle(GetApprovedTemplatesQuery request, CancellationToken cancellationToken)
    {
        // 1. Build an IQueryable base that strictly targets Approved listings
        var query = context.ProjectTemplates
            .Include(t => t.Provider)
            .Include(t => t.TemplateSkills)
                .ThenInclude(ts => ts.Skill)
            .Where(t => t.Status == ProjectTemplateStatus.Approved);

        // 2. Conditionally apply maximum academic duration filter if supplied
        if (request.MaxDurationWeeks.HasValue)
        {
            query = query.Where(t => t.ExpectedDurationWeeks <= request.MaxDurationWeeks.Value);
        }

        // 3. Conditionally apply join-table skill requirements filter if supplied
        if (request.SkillId.HasValue)
        {
            query = query.Where(t => t.TemplateSkills.Any(ts => ts.SkillId == request.SkillId.Value));
        }

        // 4. Project the filtered relational database query records directly into DTO payloads
        var result = await query
            .Select(t => new ApprovedTemplateDto
            {
                Id = t.Id,
                ProviderId = t.ProviderId,
                // FIX: Temporarily using UserId to guarantee a successful compilation.
                // Swap out '.UserId' below with your actual text property name from Provider.cs if available!
                ProviderCompanyName = t.Provider != null ? t.Provider.UserId : "Unknown Provider",
                Title = t.Title,
                Description = t.Description,
                ExpectedDurationWeeks = t.ExpectedDurationWeeks,
                Skills = t.TemplateSkills.Select(ts => new TemplateSkillDto
                {
                    Id = ts.SkillId,
                    Name = ts.Skill != null ? ts.Skill.Name : "Unknown Skill"
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        return result;
    }
}
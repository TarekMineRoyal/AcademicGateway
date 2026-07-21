using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Common.Models.AiSearch;
using AcademicGateway.Application.Features.ProjectTemplates.Queries.GetApprovedTemplates;
using AcademicGateway.Domain.ProjectTemplates.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGateway.Application.Features.Recommendations.Queries.GetProjectRecommendationsForStudent;

/// <summary>
/// Handles the execution of the <see cref="GetProjectRecommendationsForStudentQuery"/> request.
/// Extracts student profile context, requests AI vector matchmaking matches, and hydrates database records preserving rank position.
/// </summary>
public class GetProjectRecommendationsForStudentQueryHandler(
    IApplicationDbContext context,
    IAiMatchmakingClient aiClient,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetProjectRecommendationsForStudentQuery, IReadOnlyCollection<ApprovedTemplateDto>>
{
    /// <summary>
    /// Processes the project recommendation query for the currently authenticated student.
    /// </summary>
    /// <param name="request">The recommendation query containing limits.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An immutable read-only collection of recommended approved project templates sorted by relevance rank.</returns>
    public async Task<IReadOnlyCollection<ApprovedTemplateDto>> Handle(
        GetProjectRecommendationsForStudentQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Resolve authenticated user context
        if (!currentUserService.UserId.HasValue)
        {
            return Array.Empty<ApprovedTemplateDto>();
        }

        var studentId = currentUserService.UserId.Value;

        // 2. Fetch student profile context from DB
        var student = await context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken);

        if (student == null)
        {
            return Array.Empty<ApprovedTemplateDto>();
        }

        var majorName = await context.StudentMajors
            .AsNoTracking()
            .Where(sm => sm.StudentId == studentId)
            .Join(context.Majors, sm => sm.MajorId, m => m.Id, (sm, m) => m.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        var specialtyNames = await context.StudentSpecialties
            .AsNoTracking()
            .Where(ss => ss.StudentId == studentId)
            .Join(context.Specialties, ss => ss.SpecialtyId, sp => sp.Id, (ss, sp) => sp.Name)
            .ToListAsync(cancellationToken);

        var skillNames = await context.StudentSkills
            .AsNoTracking()
            .Where(ss => ss.StudentId == studentId)
            .Join(context.Skills, ss => ss.SkillId, sk => sk.Id, (ss, sk) => sk.Name)
            .ToListAsync(cancellationToken);

        // 3. Construct AI search query payload
        var queryModel = new GetProjectRecommendationsQueryModel
        {
            MajorName = majorName,
            SpecialtyNames = specialtyNames,
            SkillNames = skillNames,
            AboutMe = student.AboutMe,
            Limit = request.Limit
        };

        // 4. Invoke AI Matchmaking microservice client
        var vectorIds = await aiClient.GetProjectRecommendationsAsync(queryModel, cancellationToken);

        if (vectorIds.Count == 0)
        {
            return Array.Empty<ApprovedTemplateDto>();
        }

        // 5. Hydrate matching approved project templates from DB
        var dbEntities = await context.ProjectTemplates
            .AsNoTracking()
            .Where(t => t.Status == ProjectTemplateStatus.Approved && vectorIds.Contains(t.Id))
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
            })
            .ToListAsync(cancellationToken);

        // 6. RANK PRESERVATION INVARIANT: Re-order hydrated entities to match LanceDB index order
        var rankMap = vectorIds
            .Select((id, index) => new { id, index })
            .ToDictionary(x => x.id, x => x.index);

        return dbEntities
            .Where(x => rankMap.ContainsKey(x.Id))
            .OrderBy(x => rankMap[x.Id])
            .ToList();
    }
}
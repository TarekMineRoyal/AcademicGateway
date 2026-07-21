using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Common.Models.AiSearch;
using AcademicGateway.Application.Features.Skills.Queries.GetSkills;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGateway.Application.Features.Recommendations.Queries.GetSkillRecommendationsForStudent;

/// <summary>
/// Handles the execution of the <see cref="GetSkillRecommendationsForStudentQuery"/> request.
/// Extracts student profile context, requests AI vector skill suggestions,
/// and hydrates matching skill records while preserving rank position.
/// </summary>
public class GetSkillRecommendationsForStudentQueryHandler(
    IApplicationDbContext context,
    IAiMatchmakingClient aiClient,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetSkillRecommendationsForStudentQuery, IReadOnlyCollection<SkillDto>>
{
    /// <summary>
    /// Processes the adjacent skill recommendation query for the authenticated student profile.
    /// </summary>
    /// <param name="request">The query containing recommendation limits.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An immutable read-only collection of recommended skill records sorted by relevance rank.</returns>
    public async Task<IReadOnlyCollection<SkillDto>> Handle(
        GetSkillRecommendationsForStudentQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Resolve authenticated user context
        if (!currentUserService.UserId.HasValue)
        {
            return Array.Empty<SkillDto>();
        }

        var studentId = currentUserService.UserId.Value;

        // 2. Fetch student profile context from DB
        var student = await context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken);

        if (student == null)
        {
            return Array.Empty<SkillDto>();
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

        // 3. Build search query model
        var queryModel = new GetSkillRecommendationsQueryModel
        {
            MajorName = majorName,
            SpecialtyNames = specialtyNames,
            SkillNames = skillNames,
            AboutMe = student.AboutMe,
            Limit = request.Limit
        };

        // 4. Invoke AI Matchmaking microservice client
        var vectorIds = await aiClient.GetSkillRecommendationsAsync(queryModel, cancellationToken);

        if (vectorIds.Count == 0)
        {
            return Array.Empty<SkillDto>();
        }

        // 5. Hydrate matching skills from DB
        var dbEntities = await context.Skills
            .AsNoTracking()
            .Where(s => vectorIds.Contains(s.Id))
            .Select(s => new SkillDto
            {
                Id = s.Id,
                Name = s.Name
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
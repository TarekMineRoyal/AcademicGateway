using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Common.Models.AiSearch;
using AcademicGateway.Application.Features.Professors.Queries.SearchProfessors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGateway.Application.Features.Recommendations.Queries.GetProfessorSuggestionsForProject;

/// <summary>
/// Handles the execution of the <see cref="GetProfessorSuggestionsForProjectQuery"/> request.
/// Resolves project template context by ID, requests AI faculty suggestions,
/// and hydrates professor search result DTOs while preserving rank position.
/// </summary>
public class GetProfessorSuggestionsForProjectQueryHandler(
    IApplicationDbContext context,
    IAiMatchmakingClient aiClient,
    IIdentityService identityService)
    : IRequestHandler<GetProfessorSuggestionsForProjectQuery, IReadOnlyCollection<ProfessorSearchResultDto>>
{
    /// <summary>
    /// Processes the faculty advisor suggestion query for a project template.
    /// </summary>
    /// <param name="request">The query containing target project template identifier and limit.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>An immutable read-only collection of matching professor search records sorted by relevance rank.</returns>
    public async Task<IReadOnlyCollection<ProfessorSearchResultDto>> Handle(
        GetProfessorSuggestionsForProjectQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Resolve project template details from database
        var template = await context.ProjectTemplates
            .AsNoTracking()
            .Where(t => t.Id == request.ProjectTemplateId)
            .Select(t => new
            {
                t.Title,
                t.Description,
                MajorName = t.MajorId.HasValue
                    ? context.Majors.Where(m => m.Id == t.MajorId.Value).Select(m => m.Name).FirstOrDefault()
                    : null,
                SpecialtyName = t.SpecialtyId.HasValue
                    ? context.Specialties.Where(s => s.Id == t.SpecialtyId.Value).Select(s => s.Name).FirstOrDefault()
                    : null,
                SkillNames = t.ProjectTemplateSkills
                    .Select(pts => pts.Skill != null ? pts.Skill.Name : string.Empty)
                    .Where(n => !string.IsNullOrEmpty(n))
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (template == null || string.IsNullOrWhiteSpace(template.Title) || string.IsNullOrWhiteSpace(template.Description))
        {
            return Array.Empty<ProfessorSearchResultDto>();
        }

        // 2. Build search query model for AI engine
        var queryModel = new GetProfessorSuggestionsQueryModel
        {
            Title = template.Title,
            Description = template.Description,
            MajorName = template.MajorName,
            SpecialtyName = template.SpecialtyName,
            SkillNames = template.SkillNames,
            Limit = request.Limit
        };

        // 3. Invoke AI Matchmaking microservice client
        var vectorIds = await aiClient.GetProfessorSuggestionsAsync(queryModel, cancellationToken);

        if (vectorIds.Count == 0)
        {
            return Array.Empty<ProfessorSearchResultDto>();
        }

        // 4. Hydrate matching professors from DB including research interests
        var dbProfessors = await context.Professors
            .AsNoTracking()
            .Include(p => p.ResearchInterests)
                .ThenInclude(ri => ri.ResearchInterest)
            .Where(p => vectorIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var allIdentityProfessors = await identityService.SearchProfessorsAsync(null, cancellationToken);
        var emailLookup = allIdentityProfessors.ToDictionary(x => x.Id, x => x.Email);

        // 5. RANK PRESERVATION INVARIANT: Re-order hydrated entities to match LanceDB index order
        var rankMap = vectorIds
            .Select((id, index) => new { id, index })
            .ToDictionary(x => x.id, x => x.index);

        return dbProfessors
            .Where(p => rankMap.ContainsKey(p.Id))
            .OrderBy(p => rankMap[p.Id])
            .Select(p => new ProfessorSearchResultDto
            {
                Id = p.Id,
                FullName = p.FullName,
                Email = emailLookup.TryGetValue(p.Id, out var email) ? email : string.Empty,
                Department = p.Department,
                AboutMe = p.AboutMe,
                ResearchInterests = p.ResearchInterests
                    .Select(ri => ri.ResearchInterest != null ? ri.ResearchInterest.Area : string.Empty)
                    .Where(area => !string.IsNullOrWhiteSpace(area))
                    .ToList(),
                CurrentProjectCount = p.CurrentProjectCount,
                MaxSupervisionCapacity = p.MaxSupervisionCapacity,
                IsAcceptingProjects = p.IsAcceptingProjects
            })
            .ToList();
    }
}
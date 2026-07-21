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
/// Resolves project context from an existing blueprint or raw parameters, requests AI faculty suggestions,
/// and hydrates professor search result DTOs while preserving rank position.
/// </summary>
public class GetProfessorSuggestionsForProjectQueryHandler(
    IApplicationDbContext context,
    IAiMatchmakingClient aiClient,
    IIdentityService identityService)
    : IRequestHandler<GetProfessorSuggestionsForProjectQuery, IReadOnlyCollection<ProfessorSearchResultDto>>
{
    /// <summary>
    /// Processes the faculty advisor suggestion query for a project blueprint.
    /// </summary>
    /// <param name="request">The query containing project context or template identifier.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>An immutable read-only collection of matching professor search records sorted by relevance rank.</returns>
    public async Task<IReadOnlyCollection<ProfessorSearchResultDto>> Handle(
        GetProfessorSuggestionsForProjectQuery request,
        CancellationToken cancellationToken)
    {
        string title = request.Title ?? string.Empty;
        string description = request.Description ?? string.Empty;
        string? majorName = request.MajorName;
        string? specialtyName = request.SpecialtyName;
        List<string>? skillNames = request.SkillNames;

        // 1. If TemplateId is provided, resolve project context from database
        if (request.TemplateId.HasValue)
        {
            var template = await context.ProjectTemplates
                .AsNoTracking()
                .Where(t => t.Id == request.TemplateId.Value)
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

            if (template != null)
            {
                title = string.IsNullOrWhiteSpace(title) ? template.Title : title;
                description = string.IsNullOrWhiteSpace(description) ? template.Description : description;
                majorName ??= template.MajorName;
                specialtyName ??= template.SpecialtyName;
                skillNames ??= template.SkillNames;
            }
        }

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
        {
            return Array.Empty<ProfessorSearchResultDto>();
        }

        // 2. Build search query model
        var queryModel = new GetProfessorSuggestionsQueryModel
        {
            Title = title,
            Description = description,
            MajorName = majorName,
            SpecialtyName = specialtyName,
            SkillNames = skillNames,
            Limit = request.Limit
        };

        // 3. Invoke AI Matchmaking microservice client
        var vectorIds = await aiClient.GetProfessorSuggestionsAsync(queryModel, cancellationToken);

        if (vectorIds.Count == 0)
        {
            return Array.Empty<ProfessorSearchResultDto>();
        }

        // 4. Hydrate matching professors from DB
        var dbProfessors = await context.Professors
            .AsNoTracking()
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
                Email = emailLookup.TryGetValue(p.Id, out var email) ? email : string.Empty
            })
            .ToList();
    }
}
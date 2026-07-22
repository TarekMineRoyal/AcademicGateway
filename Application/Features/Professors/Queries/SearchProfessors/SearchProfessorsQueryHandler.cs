using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Professors.Queries.SearchProfessors;

/// <summary>
/// Handles the execution of the <see cref="SearchProfessorsQuery"/> autocomplete request.
/// Leverages the identity subsystem for string matching user accounts, then hydrates
/// domain-level faculty profile metadata, research interests, and project capacity.
/// </summary>
public class SearchProfessorsQueryHandler(
    IIdentityService identityService,
    IApplicationDbContext context)
    : IRequestHandler<SearchProfessorsQuery, PaginatedResult<ProfessorSearchResultDto>>
{
    /// <summary>
    /// Processes the incoming search query by dispatching search filtering to the identity service layer
    /// and populating domain metadata for the matching professor records.
    /// </summary>
    /// <param name="request">The incoming structural query envelope wrapping the search filtering and pagination constraints.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>A paginated result containing matching, fully hydrated professor search records.</returns>
    public async Task<PaginatedResult<ProfessorSearchResultDto>> Handle(
        SearchProfessorsQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch paginated search results from Identity Service
        var paginatedResult = await identityService.SearchProfessorsAsync(
            request.SearchTerm,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        if (paginatedResult.Items.Count == 0)
        {
            return paginatedResult;
        }

        // 2. Resolve matching domain professor profiles from the database
        var professorIds = paginatedResult.Items.Select(p => p.Id).ToList();

        var professors = await context.Professors
            .AsNoTracking()
            .Include(p => p.ResearchInterests)
                .ThenInclude(ri => ri.ResearchInterest)
            .Where(p => professorIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        // 3. Hydrate domain fields onto each result DTO item
        foreach (var item in paginatedResult.Items)
        {
            if (professors.TryGetValue(item.Id, out var professor))
            {
                item.Department = professor.Department;
                item.AboutMe = professor.AboutMe;
                item.ResearchInterests = professor.ResearchInterests
                    .Select(ri => ri.ResearchInterest != null ? ri.ResearchInterest.Area : string.Empty)
                    .Where(area => !string.IsNullOrWhiteSpace(area))
                    .ToList();
                item.CurrentProjectCount = professor.CurrentProjectCount;
                item.MaxSupervisionCapacity = professor.MaxSupervisionCapacity;
                item.IsAcceptingProjects = professor.IsAcceptingProjects;
            }
        }

        return paginatedResult;
    }
}
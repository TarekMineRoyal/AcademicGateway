using AcademicGateway.Application.Common.Models;
using MediatR;

namespace AcademicGateway.Application.Features.Professors.Queries.SearchProfessors;

/// <summary>
/// MediatR query parameter envelope passing criteria down to the faculty directory search engine.
/// Wraps optional search parameters used to discover matching professor profiles.
/// </summary>
/// <param name="SearchTerm">The optional structural keyword segment matched case-insensitively against user details.</param>
/// <param name="PageNumber">The 1-based index of the page to retrieve (default: 1).</param>
/// <param name="PageSize">The maximum number of items to retrieve per page (default: 10).</param>
public record SearchProfessorsQuery(
    string? SearchTerm = null,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PaginatedResult<ProfessorSearchResultDto>>;
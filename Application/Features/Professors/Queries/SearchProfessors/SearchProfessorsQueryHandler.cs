using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Common.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Professors.Queries.SearchProfessors;

/// <summary>
/// Handles the execution of the <see cref="SearchProfessorsQuery"/> autocomplete request.
/// Leverages the identity subsystem to run cross-cutting string matching across user accounts and profile records.
/// </summary>
public class SearchProfessorsQueryHandler(IIdentityService identityService)
    : IRequestHandler<SearchProfessorsQuery, PaginatedResult<ProfessorSearchResultDto>>
{
    /// <summary>
    /// Processes the incoming search query by dispatching the search filtering and pagination rules to the identity service layer.
    /// </summary>
    /// <param name="request">The incoming structural query envelope wrapping the search filtering and pagination constraints.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>A paginated result containing matching presentational professor search records.</returns>
    public async Task<PaginatedResult<ProfessorSearchResultDto>> Handle(
        SearchProfessorsQuery request,
        CancellationToken cancellationToken)
    {
        return await identityService.SearchProfessorsAsync(
            request.SearchTerm,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
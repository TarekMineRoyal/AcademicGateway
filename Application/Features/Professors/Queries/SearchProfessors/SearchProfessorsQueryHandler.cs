using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Professors.Queries.SearchProfessors;

/// <summary>
/// Handles the execution of the <see cref="SearchProfessorsQuery"/> autocomplete request.
/// Leverages the identity subsystem to run cross-cutting string matching across user accounts and profile records.
/// </summary>
public class SearchProfessorsQueryHandler(IIdentityService identityService)
    : IRequestHandler<SearchProfessorsQuery, IReadOnlyCollection<ProfessorSearchResultDto>>
{
    /// <summary>
    /// Processes the incoming search query by dispatching the search filtering rules to the identity service layer.
    /// </summary>
    /// <param name="request">The incoming structural query envelope wrapping the search filtering constraints.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>An immutable read-only sequence containing matching presentational professor search records.</returns>
    public async Task<IReadOnlyCollection<ProfessorSearchResultDto>> Handle(
        SearchProfessorsQuery request,
        CancellationToken cancellationToken)
    {
        return await identityService.SearchProfessorsAsync(request.SearchTerm, cancellationToken);
    }
}
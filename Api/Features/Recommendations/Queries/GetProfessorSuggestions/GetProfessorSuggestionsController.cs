using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicGateway.Application.Features.Professors.Queries.SearchProfessors;
using AcademicGateway.Application.Features.Recommendations.Queries.GetProfessorSuggestionsForProject;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGateway.Api.Features.Recommendations.Queries.GetProfessorSuggestions;

/// <summary>
/// Endpoint for suggesting faculty advisors based on project blueprint context.
/// </summary>
[ApiController]
[Tags("Recommendations")]
[Authorize(Roles = "Provider,Admin")]
[Route("api/v1/recommendations/professors")]
public class GetProfessorSuggestionsController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Accepts project template context parameters or an existing template ID and returns matching faculty advisor suggestions.
    /// </summary>
    /// <param name="query">The project blueprint details or template reference query.</param>
    /// <returns>A 200 OK response containing ranked professor search results.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(IReadOnlyCollection<ProfessorSearchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetProfessorSuggestions([FromBody] GetProfessorSuggestionsForProjectQuery query)
    {
        var suggestions = await mediator.Send(query);

        return Ok(suggestions);
    }
}
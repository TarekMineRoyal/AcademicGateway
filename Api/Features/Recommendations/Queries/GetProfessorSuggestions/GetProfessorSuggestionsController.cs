using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicGateway.Domain.Common.Constants;
using AcademicGateway.Application.Features.Professors.Queries.SearchProfessors;
using AcademicGateway.Application.Features.Recommendations.Queries.GetProfessorSuggestionsForProject;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGateway.Api.Features.Recommendations.Queries.GetProfessorSuggestions;

/// <summary>
/// Endpoint for suggesting faculty advisors based on a project template blueprint.
/// </summary>
[ApiController]
[Tags("Recommendations")]
[Authorize(Roles = Roles.Student)]
[Route("api/v1/recommendations/professors")]
public class GetProfessorSuggestionsController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Retrieves a ranked list of suggested faculty advisors for a given project template.
    /// </summary>
    /// <param name="projectTemplateId">The unique identifier of the target project template.</param>
    /// <param name="limit">The maximum number of professor suggestions to return (default: 10).</param>
    /// <returns>A 200 OK response containing ranked professor search results.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<ProfessorSearchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetProfessorSuggestions(
        [FromQuery] Guid projectTemplateId,
        [FromQuery] int limit = 10)
    {
        var query = new GetProfessorSuggestionsForProjectQuery
        {
            ProjectTemplateId = projectTemplateId,
            Limit = limit
        };

        var suggestions = await mediator.Send(query);

        return Ok(suggestions);
    }
}
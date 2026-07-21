using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicGateway.Application.Features.ProjectTemplates.Queries.GetApprovedTemplates;
using AcademicGateway.Application.Features.Recommendations.Queries.GetProjectRecommendationsForStudent;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGateway.Api.Features.Recommendations.Queries.GetProjectRecommendations;

/// <summary>
/// Endpoint for fetching AI-powered project template recommendations for authenticated students.
/// </summary>
[ApiController]
[Tags("Recommendations")]
[Authorize(Roles = "Student")]
[Route("api/v1/recommendations/projects")]
public class GetProjectRecommendationsController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Retrieves a ranked list of recommended approved project templates tailored to the student's profile context.
    /// </summary>
    /// <param name="limit">The maximum number of recommended templates to return (default: 10).</param>
    /// <returns>A 200 OK response containing vector-ranked project template recommendations.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<ApprovedTemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetProjectRecommendations([FromQuery] int limit = 10)
    {
        var query = new GetProjectRecommendationsForStudentQuery { Limit = limit };
        var recommendations = await mediator.Send(query);

        return Ok(recommendations);
    }
}
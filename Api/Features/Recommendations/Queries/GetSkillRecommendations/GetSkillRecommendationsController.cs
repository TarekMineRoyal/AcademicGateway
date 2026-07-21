using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicGateway.Application.Features.Recommendations.Queries.GetSkillRecommendationsForStudent;
using AcademicGateway.Application.Features.Skills.Queries.GetSkills;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGateway.Api.Features.Recommendations.Queries.GetSkillRecommendations;

/// <summary>
/// Endpoint for fetching adjacent technical skill recommendations for student profile development.
/// </summary>
[ApiController]
[Tags("Recommendations")]
[Authorize(Roles = "Student")]
[Route("api/v1/recommendations/skills")]
public class GetSkillRecommendationsController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Retrieves a ranked list of recommended adjacent skills based on the student's current profile competencies.
    /// </summary>
    /// <param name="limit">The maximum number of recommended skills to return (default: 10).</param>
    /// <returns>A 200 OK response containing vector-ranked skill recommendations.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<SkillDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSkillRecommendations([FromQuery] int limit = 10)
    {
        var query = new GetSkillRecommendationsForStudentQuery { Limit = limit };
        var skills = await mediator.Send(query);

        return Ok(skills);
    }
}
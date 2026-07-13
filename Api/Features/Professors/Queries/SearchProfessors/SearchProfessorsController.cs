using AcademicGateway.Application.Features.Professors.Queries.SearchProfessors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.Professors.Queries.SearchProfessors;

/// <summary>
/// Endpoint for discovering institutional faculty member accounts.
/// </summary>
[Authorize]
[ApiController]
[Tags("Professors")]
[Route("api/professors")]
public class SearchProfessorsController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Searches across available professor security accounts using partial text matching across full names, emails, or usernames.
    /// </summary>
    /// <param name="searchTerm">The optional keyword segment used to filter the faculty collection directory.</param>
    /// <returns>An array list containing lightweight presentational professor lookup records.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<ProfessorSearchResultDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SearchProfessors([FromQuery] string? searchTerm)
    {
        var result = await mediator.Send(new SearchProfessorsQuery(searchTerm));
        return Ok(result);
    }
}
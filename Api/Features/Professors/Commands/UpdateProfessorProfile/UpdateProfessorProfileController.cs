using AcademicGateway.Application.Features.Professors.Commands.UpdateProfessorProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.Professors.Commands.UpdateProfessorProfile;

/// <summary>
/// Endpoint for managing active faculty profile lifecycles and maintenance operations.
/// </summary>
[ApiController]
[Tags("Professors")]
[Authorize(Roles = "Professor")]
[Route("api/professors")]
public class UpdateProfessorProfileController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Updates the currently authenticated professor's profile details, legal naming alignments, department divisions, rank status, and active research interests.
    /// </summary>
    /// <param name="command">The updated profile criteria and research specialty selection collection matrix payload.</param>
    /// <returns>A 204 No Content response indicating a successful atomic state synchronization.</returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfessorProfileCommand command)
    {
        await mediator.Send(command);

        // Returns a standard 204 No Content indicating successful resource modification with an empty body
        return NoContent();
    }
}
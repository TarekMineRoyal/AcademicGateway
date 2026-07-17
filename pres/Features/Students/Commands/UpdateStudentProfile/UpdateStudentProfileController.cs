using AcademicGateway.Application.Features.Students.Commands.UpdateStudentProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.Students.Commands.UpdateStudentProfile;

/// <summary>
/// Endpoint for managing active student profile lifecycles and maintenance operations.
/// </summary>
[ApiController]
[Tags("Students")]
[Authorize(Roles = "Student")]
[Route("api/students")]
public class UpdateStudentProfileController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Updates the currently authenticated student's profile details, academic majors, specialties, and technical skills.
    /// </summary>
    /// <param name="command">The updated profile criteria and relational selection collection matrix payload.</param>
    /// <returns>A 204 No Content response indicating a successful atomic state synchronization.</returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateStudentProfileCommand command)
    {
        await mediator.Send(command);

        // Returns a standard 204 No Content indicating successful resource modification with an empty body
        return NoContent();
    }
}
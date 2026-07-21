using AcademicGateway.Application.Features.Professors.Commands.UpdateProfessorProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.Professors.Commands.UpdateProfessorProfile;

/// <summary>
/// Presentation layer request schema for updating academic professor profile parameters.
/// </summary>
public record UpdateProfessorProfileRequest(
    string FullName,
    string Department,
    string Rank,
    IReadOnlyCollection<Guid> ResearchInterestIds,
    string? AboutMe);

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
    /// <param name="request">The presentational request payload containing the updated faculty organization parameters and descriptive summaries.</param>
    /// <returns>A 204 No Content response indicating a successful atomic state synchronization.</returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfessorProfileRequest request)
    {
        // Explicitly map presentation request primitives to the inner MediatR command to protect the public contract boundary
        var command = new UpdateProfessorProfileCommand
        {
            FullName = request.FullName,
            Department = request.Department,
            Rank = request.Rank,
            ResearchInterestIds = request.ResearchInterestIds,
            AboutMe = request.AboutMe
        };

        await mediator.Send(command);

        // Returns a standard 204 No Content indicating successful resource modification with an empty body
        return NoContent();
    }
}
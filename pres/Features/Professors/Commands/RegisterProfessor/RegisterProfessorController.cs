using AcademicGateway.Application.Features.Professors.Commands.RegisterProfessor;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGateway.Api.Features.Professors.Commands.RegisterProfessor;

/// <summary>
/// Endpoint for registering new institutional faculty members.
/// </summary>
[ApiController]
[Tags("Professors")]
[AllowAnonymous]
[Route("api/professors")]
public class RegisterProfessorController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Registers a new professor, provisions their identity security credentials, and creates their domain profile.
    /// </summary>
    /// <param name="command">The professor registration details payload.</param>
    /// <returns>The unique identifier generated for the newly initialized professor aggregate.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterProfessorCommand command)
    {
        var professorId = await mediator.Send(command);

        // Returns a 201 Created state alongside the encapsulated resource tracking identifier
        return Created(string.Empty, new { Id = professorId });
    }
}
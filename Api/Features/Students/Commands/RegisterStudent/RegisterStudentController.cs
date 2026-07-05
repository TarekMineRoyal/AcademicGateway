using AcademicGateway.Application.Features.Students.Commands.RegisterStudent;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.Students.Commands.RegisterStudent;

/// <summary>
/// Endpoint for registering new students.
/// </summary>
[ApiController]
[Tags("Students")]
[Route("api/students")]
public class RegisterStudentController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Registers a new student account and initializes their platform aggregate profile.
    /// </summary>
    /// <param name="command">The student registration credential and profile payload details.</param>
    /// <returns>A 201 Created response carrying the primary unique identifier generated for the student.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterStudentCommand command)
    {
        var studentId = await mediator.Send(command);

        // Returns a standard 201 Created collection status tracking response
        return Created(string.Empty, new { Id = studentId });
    }
}
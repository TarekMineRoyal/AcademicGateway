using AcademicGateway.Application.Features.Students.Commands.RegisterStudent;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGateway.Api.Features.Students.Register;

[ApiController]
[Route("api/auth")]
public class RegisterStudentController(ISender mediator) : ControllerBase
{
    [HttpPost("register/student")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterStudentCommand command)
    {
        var userId = await mediator.Send(command);
        return Ok(new { UserId = userId, Message = "Student registered successfully." });
    }
}
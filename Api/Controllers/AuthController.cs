using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Providers.Commands.RegisterProvider;
using Application.Features.Professors.Commands.RegisterProfessor;
using Application.Features.Auth.Commands.Login;
using Application.Features.Students.Commands.RegisterStudent;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ISender mediator) : ControllerBase
{
    [HttpPost("register/student")]
    public async Task<IActionResult> RegisterStudent([FromBody] RegisterStudentCommand command)
    {
        var userId = await mediator.Send(command);
        return Ok(new { UserId = userId, Message = "Student registered successfully." });
    }

    [HttpPost("register/provider")]
    public async Task<IActionResult> RegisterProvider([FromBody] RegisterProviderCommand command)
    {
        var userId = await mediator.Send(command);
        return Ok(new { UserId = userId, Message = "Provider registered successfully." });
    }

    [HttpPost("register/professor")]
    public async Task<IActionResult> RegisterProfessor([FromBody] RegisterProfessorCommand command)
    {
        var userId = await mediator.Send(command);
        return Ok(new { UserId = userId, Message = "Professor registered successfully." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var token = await mediator.Send(command);

        if (token == null)
            return Unauthorized(new { Message = "Invalid email or password." });

        return Ok(new { Token = token });
    }
}
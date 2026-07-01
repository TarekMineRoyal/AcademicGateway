using MediatR;
using Microsoft.AspNetCore.Mvc;
using AcademicGateway.Application.Features.Providers.Commands.RegisterProvider;
using AcademicGateway.Application.Features.Professors.Commands.RegisterProfessor;
using AcademicGateway.Application.Features.Auth.Commands.Login;
using AcademicGateway.Application.Features.Students.Commands.RegisterStudent;
using Microsoft.AspNetCore.Http;

namespace AcademicGateway.Api.Controllers.Auth;

/// <summary>
/// Manages user authentication and account registration workflows.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Registers a new student account.
    /// </summary>
    [HttpPost("register/student")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterStudent([FromBody] RegisterStudentCommand command)
    {
        var userId = await mediator.Send(command);
        return Ok(new { UserId = userId, Message = "Student registered successfully." });
    }

    /// <summary>
    /// Registers a new corporate provider account.
    /// </summary>
    [HttpPost("register/provider")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterProvider([FromBody] RegisterProviderCommand command)
    {
        var userId = await mediator.Send(command);
        return Ok(new { UserId = userId, Message = "Provider registered successfully." });
    }

    /// <summary>
    /// Registers a new professor account.
    /// </summary>
    [HttpPost("register/professor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterProfessor([FromBody] RegisterProfessorCommand command)
    {
        var userId = await mediator.Send(command);
        return Ok(new { UserId = userId, Message = "Professor registered successfully." });
    }

    /// <summary>
    /// Authenticates a user and issues an access token.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var token = await mediator.Send(command);

        if (string.IsNullOrEmpty(token))
            return Unauthorized(new { Message = "Invalid email or password." });

        return Ok(new { Token = token });
    }
}
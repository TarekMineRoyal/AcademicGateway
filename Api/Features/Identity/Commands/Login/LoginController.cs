using AcademicGateway.Application.Features.Identity.Commands.Login;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGateway.Api.Features.Identity.Commands.Login;

/// <summary>
/// Endpoint for user authentication and session management.
/// </summary>
[ApiController]
[Tags("Identity")]
[AllowAnonymous]
[Route("api/auth")]
public class LoginController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Authenticates a user with credentials and generates a secure JSON Web Token (JWT).
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
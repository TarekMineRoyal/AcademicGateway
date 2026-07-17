using AcademicGateway.Application.Features.Identity.Commands.Login;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
    /// <param name="command">The application command container carrying user credentials.</param>
    /// <returns>A 200 OK status containing the token on success, or a 401 Unauthorized ProblemDetails response on failure.</returns>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var token = await mediator.Send(command);

        if (string.IsNullOrEmpty(token))
        {
            // Standardize failure payload shapes to comply with global RFC-7807 ProblemDetails middleware boundaries
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "Invalid email or password."
            });
        }

        return Ok(new { Token = token });
    }
}
using AcademicGateway.Application.Features.Providers.Commands.RegisterProvider;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGateway.Api.Features.Providers.Register;

[ApiController]
[Route("api/auth")]
public class RegisterProviderController(ISender mediator) : ControllerBase
{
    [HttpPost("register/provider")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterProviderCommand command)
    {
        var userId = await mediator.Send(command);
        return Ok(new { UserId = userId, Message = "Provider registered successfully." });
    }
}
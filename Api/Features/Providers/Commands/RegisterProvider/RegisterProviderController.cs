using AcademicGateway.Application.Features.Providers.Commands.RegisterProvider;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.Providers.Commands.RegisterProvider;

/// <summary>
/// Endpoint for registering new corporate industry providers.
/// </summary>
[ApiController]
[Tags("Providers")]
[Route("api/providers")]
public class RegisterProviderController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Registers a new corporate provider account and initializes their platform aggregate profile.
    /// </summary>
    /// <param name="command">The provider registration credential and profile payload details.</param>
    /// <returns>A 201 Created response carrying the primary unique identifier generated for the provider.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterProviderCommand command)
    {
        var providerId = await mediator.Send(command);

        // Returns a standard 201 Created collection status tracking response
        return Created(string.Empty, new { Id = providerId });
    }
}
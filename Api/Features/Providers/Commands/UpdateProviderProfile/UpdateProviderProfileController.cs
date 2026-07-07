using AcademicGateway.Application.Features.Providers.Commands.UpdateProviderProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.Providers.Commands.UpdateProviderProfile;

/// <summary>
/// Endpoint for managing active corporate provider profile lifecycles and maintenance operations.
/// </summary>
[ApiController]
[Tags("Providers")]
[Authorize(Roles = "Provider")]
[Route("api/providers")]
public class UpdateProviderProfileController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Updates the currently authenticated provider's public-facing branding information, description text, and official verification URL.
    /// </summary>
    /// <param name="command">The updated organization parameters and descriptive summary payload criteria.</param>
    /// <returns>A 204 No Content response indicating successful atomic state synchronization.</returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProviderProfileCommand command)
    {
        await mediator.Send(command);

        // Returns a standard 204 No Content indicating successful resource modification with an empty body
        return NoContent();
    }
}
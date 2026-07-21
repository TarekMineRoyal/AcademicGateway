using AcademicGateway.Application.Features.Providers.Commands.UpdateProviderProfile;
using AcademicGateway.Domain.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.Providers.Commands.UpdateProviderProfile;

/// <summary>
/// Presentation layer request schema for updating corporate provider profile parameters.
/// </summary>
public record UpdateProviderProfileRequest(
    string CompanyName,
    string CompanyDescription,
    string WebsiteUrl);

/// <summary>
/// Endpoint for managing active corporate provider profile lifecycles and maintenance operations.
/// </summary>
[ApiController]
[Tags("Providers")]
[Authorize(Roles = Roles.Provider)]
[Route("api/providers")]
public class UpdateProviderProfileController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Updates the currently authenticated provider's public-facing branding information, description text, and official verification URL.
    /// </summary>
    /// <param name="request">The presentational request payload containing the updated organization parameters and descriptive summaries.</param>
    /// <returns>A 204 No Content response indicating successful atomic state synchronization.</returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProviderProfileRequest request)
    {
        // Explicitly map presentation request primitives to the inner MediatR command to protect the public contract boundary
        var command = new UpdateProviderProfileCommand
        {
            CompanyName = request.CompanyName,
            CompanyDescription = request.CompanyDescription,
            WebsiteUrl = request.WebsiteUrl
        };

        await mediator.Send(command);

        // Returns a standard 204 No Content indicating successful resource modification with an empty body
        return NoContent();
    }
}
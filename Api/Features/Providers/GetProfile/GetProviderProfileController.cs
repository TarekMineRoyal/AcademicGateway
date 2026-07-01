using AcademicGateway.Application.Features.Providers.Queries.GetProviderProfile;
using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGateway.Api.Features.Providers.GetProfile;

/// <summary>
/// Endpoint for retrieving the authenticated provider profile.
/// </summary>
[Authorize(Roles = "Provider")]
[ApiController]
[Route("api/providers")]
public class GetProviderProfileController(ISender mediator, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProviderProfile()
    {
        if (!currentUserService.UserId.HasValue) return Unauthorized();

        var result = await mediator.Send(new GetProviderProfileQuery(currentUserService.UserId.Value));
        return Ok(result);
    }
}
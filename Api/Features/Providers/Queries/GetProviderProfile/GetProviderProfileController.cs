using AcademicGateway.Application.Features.Providers.Queries.GetProviderProfile;
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.Providers.Queries.GetProviderProfile;

/// <summary>
/// Endpoint for retrieving the authenticated provider profile.
/// </summary>
[Authorize(Roles = Roles.Provider)]
[ApiController]
[Tags("Providers")]
[Route("api/providers/profile")]
public class GetProviderProfileController(ISender mediator, ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Retrieves the profile details of the currently authenticated corporate provider.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProviderProfile()
    {
        if (!currentUserService.UserId.HasValue) return Unauthorized();

        var result = await mediator.Send(new GetProviderProfileQuery(currentUserService.UserId.Value));
        return Ok(result);
    }
}
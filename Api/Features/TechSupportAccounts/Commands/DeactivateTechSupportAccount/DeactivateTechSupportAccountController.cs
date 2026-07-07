using AcademicGateway.Application.Features.TechSupportAccounts.Commands.DeactivateTechSupportAccount;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.TechSupportAccounts.Commands.DeactivateTechSupportAccount;

/// <summary>
/// Endpoint for managing the lifecycles and secure off-boarding paths of provider tech support accounts.
/// </summary>
[ApiController]
[Tags("Tech Support Accounts")]
[Authorize(Roles = "Provider")]
[Route("api/providers/tech-support")]
public class DeactivateTechSupportAccountController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Securely deactivates a managed corporate technician account, immediately disabling credentials and platform access bounds.
    /// </summary>
    /// <param name="id">The unique identification surrogate key of the target tech support account to revoke.</param>
    /// <returns>A 204 No Content response indicating successful account suspension and off-boarding.</returns>
    [HttpPut("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeactivateAccount([FromRoute] Guid id)
    {
        // Bind the route identity parameter token onto the underlying CQRS message definition payload
        await mediator.Send(new DeactivateTechSupportAccountCommand { TechSupportAccountId = id });

        // Returns a standard 204 No Content indicating successful resource state modification with an empty body
        return NoContent();
    }
}
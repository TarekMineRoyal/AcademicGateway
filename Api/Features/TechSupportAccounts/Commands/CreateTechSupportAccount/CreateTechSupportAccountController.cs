using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.TechSupportAccounts.Commands.CreateTechSupportAccount;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.TechSupportAccounts.Commands.CreateTechSupportAccount;

/// <summary>
/// API Request payload schema for initializing a secondary corporate technical support profile.
/// </summary>
public record CreateTechSupportRequest(string Email, string Password);

/// <summary>
/// Single Action Controller endpoint allowing authenticated corporate providers 
/// to provision delegated technical support accounts under their organizational umbrella.
/// </summary>
[ApiController]
[Tags("Tech Support Accounts")]
[Authorize(Roles = "Provider")] // Enforce that only verified corporate accounts can allocate support resources
[Route("api/tech-support-accounts")]
public class CreateTechSupportAccountController(
    ISender mediator,
    ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Provisions a new technical support account tied to the authenticated corporate provider.
    /// </summary>
    /// <param name="request">The login credentials and contact details payload for the support profile.</param>
    /// <returns>A 201 Created response carrying the primary tracking identifier generated for the support account.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateTechSupport([FromBody] CreateTechSupportRequest request)
    {
        // Guard Invariant: Ensure the corporate account authentication state is fully validated
        if (!currentUserService.UserId.HasValue)
        {
            return Unauthorized("Provider corporate security context could not be resolved.");
        }

        // Hydrate the CQRS command object securely mapping token credentials and payload attributes
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = currentUserService.UserId.Value,
            Email = request.Email,
            Password = request.Password
        };

        var techAccountId = await mediator.Send(command);

        // Stream back a standardized 201 Created resource collection footprint mapping payload
        return Created(string.Empty, new { Id = techAccountId });
    }
}
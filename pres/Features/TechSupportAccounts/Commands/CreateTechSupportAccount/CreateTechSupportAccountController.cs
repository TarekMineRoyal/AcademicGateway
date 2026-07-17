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
/// <param name="Email">The institutional or corporate email address to serve as user identity and contact point.</param>
/// <param name="Password">The plain-text requested initial password for credential configuration.</param>
/// <param name="StaffNumber">The provider-assigned unique employee identification number code for corporate auditing.</param>
/// <param name="SupportTier">The operational tier or access assignment level designation (e.g., "Tier 1 Helpdesk", "Mentor").</param>
public record CreateTechSupportRequest(
    string Email,
    string Password,
    string StaffNumber,
    string SupportTier);

/// <summary>
/// Single Action Controller endpoint allowing authenticated corporate industry providers 
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
    /// Provisions a new technical support account securely tied to the authenticated corporate provider.
    /// </summary>
    /// <param name="request">The structural payload containing credentials, corporate identity, and support tier details.</param>
    /// <returns>A 201 Created response carrying the primary tracking identifier generated for the support account.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateTechSupport([FromBody] CreateTechSupportRequest request)
    {
        // Guard Invariant: Ensure the corporate account authentication state is fully validated
        if (!currentUserService.UserId.HasValue)
        {
            return Unauthorized("Provider corporate security context could not be resolved.");
        }

        // Hydrate the CQRS command object, completely mapping token credentials and all incoming request properties
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = currentUserService.UserId.Value,
            Email = request.Email,
            Password = request.Password,
            StaffNumber = request.StaffNumber,
            SupportTier = request.SupportTier
        };

        // Dispatch via MediatR bus to trigger the application and domain tier workflows
        var techAccountId = await mediator.Send(command);

        // Stream back a standardized 201 Created resource footprint carrying the domain tracking ID
        return Created(string.Empty, new { Id = techAccountId });
    }
}
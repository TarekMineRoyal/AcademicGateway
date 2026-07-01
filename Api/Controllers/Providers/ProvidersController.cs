using AcademicGateway.Application.Features.Providers.Commands.SubmitApplication;
using AcademicGateway.Application.Features.TechSupportAccounts.Commands.CreateTechSupportAccount;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Api.Controllers.Providers;

/// <summary>
/// Manages corporate provider registration, compliance vetting, and auxiliary account management.
/// </summary>
[Authorize(Roles = "Provider")]
[ApiController]
[Route("api/[controller]")]
public class ProvidersController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Submits a formal registration and compliance vetting application for verification.
    /// </summary>
    /// <param name="request">The submission details including company info and document links.</param>
    /// <returns>The unique identifier of the submitted application.</returns>
    [HttpPost("applications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> SubmitApplication([FromBody] ApiSubmitApplicationRequest request)
    {
        if (!TryGetProviderId(out var providerId))
        {
            return Unauthorized("Provider corporate security context could not be resolved from token metadata.");
        }

        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = providerId,
            CompanyDetails = request.CompanyDetails,
            VerificationDocumentsUrl = request.VerificationDocumentsUrl
        };

        var applicationId = await mediator.Send(command);
        return Ok(applicationId);
    }

    /// <summary>
    /// Provisions a new auxiliary technical support mentor sub-account managed by this corporate provider.
    /// </summary>
    /// <param name="request">The account details for the new support member.</param>
    /// <returns>The unique identifier of the created tech support account.</returns>
    [HttpPost("tech-support")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateTechSupport([FromBody] ApiCreateTechSupportRequest request)
    {
        if (!TryGetProviderId(out var providerId))
        {
            return Unauthorized("Provider corporate security context could not be resolved from token metadata.");
        }

        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = providerId,
            Email = request.Email,
            Password = request.Password
        };

        var techAccountId = await mediator.Send(command);
        return Ok(techAccountId);
    }

    /// <summary>
    /// Helper to extract and parse the Provider ID from the current claims principal.
    /// </summary>
    private bool TryGetProviderId(out Guid providerId)
    {
        var providerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(providerIdString, out providerId);
    }
}

public record ApiSubmitApplicationRequest(string CompanyDetails, string VerificationDocumentsUrl);
public record ApiCreateTechSupportRequest(string Email, string Password, string FullName);
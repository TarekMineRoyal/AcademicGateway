using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProviderApplications.Commands.ResubmitProviderApplication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProviderApplications.Commands.ResubmitProviderApplication;

/// <summary>
/// API Request payload schema for updating and resubmitting a rejected corporate provider onboarding verification application.
/// </summary>
public record ResubmitApplicationRequest(string CompanyDetails, string VerificationDocumentsUrl);

/// <summary>
/// Single Action Controller endpoint allowing authenticated corporate providers to update 
/// and resubmit their onboarding verification application after a compliance review pushback.
/// </summary>
[ApiController]
[Tags("Provider Applications")]
[Authorize(Roles = "Provider")] // Enforce compliance security context constraints
[Route("api/provider-applications")]
public class ResubmitProviderApplicationController(
    ISender mediator,
    ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Modifies and resubmits a previously rejected provider onboarding verification request, initializing a new compliance evaluation cycle.
    /// </summary>
    /// <param name="request">The payload containing updated corporate background details and secure documentation link references.</param>
    /// <returns>A 200 OK response carrying the primary tracking identifier of the resubmitted compliance application record.</returns>
    [HttpPut("resubmit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ResubmitApplication([FromBody] ResubmitApplicationRequest request)
    {
        // Guard Invariant: Ensure the corporate user's authentication context tracking state is fully validated
        if (!currentUserService.UserId.HasValue)
        {
            return Unauthorized("Provider corporate security context could not be resolved.");
        }

        // Hydrate the CQRS command object securely mapping path, body, and token metadata parameters
        // Leveraging the authenticated token ID blocks resource enumeration and parameter tampering attacks.
        var command = new ResubmitProviderApplicationCommand
        {
            ProviderId = currentUserService.UserId.Value,
            CompanyDetails = request.CompanyDetails,
            VerificationDocumentsUrl = request.VerificationDocumentsUrl
        };

        var applicationId = await mediator.Send(command);

        // Stream back a standardized 200 OK confirmation payload tracking footprint
        return Ok(new { Id = applicationId });
    }
}
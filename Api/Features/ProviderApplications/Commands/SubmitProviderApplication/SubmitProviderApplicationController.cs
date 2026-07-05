using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProviderApplications.Commands.SubmitProviderApplication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.ProviderApplications.Commands.SubmitProviderApplication;

/// <summary>
/// API Request payload schema for submitting an onboarding verification application for a corporate provider profile.
/// </summary>
public record SubmitApplicationRequest(string CompanyDetails, string VerificationDocumentsUrl);

/// <summary>
/// Single Action Controller endpoint allowing authenticated corporate providers to submit 
/// or resubmit an onboarding verification application for platform admittance.
/// </summary>
[ApiController]
[Tags("Provider Applications")]
[Authorize(Roles = "Provider")] // Enforce compliance security context constraints
[Route("api/provider-applications")]
public class SubmitProviderApplicationController(
    ISender mediator,
    ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Submits a provider onboarding verification request and starts a new compliance evaluation cycle.
    /// </summary>
    /// <param name="request">The payload containing corporate background details and secure documentation link references.</param>
    /// <returns>A 201 Created response carrying the primary tracking identifier of the initialized compliance application record.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SubmitApplication([FromBody] SubmitApplicationRequest request)
    {
        // Guard Invariant: Ensure the corporate user's authentication context tracking state is fully validated
        if (!currentUserService.UserId.HasValue)
        {
            return Unauthorized("Provider corporate security context could not be resolved.");
        }

        // Hydrate the CQRS command object securely mapping path, body, and token metadata parameters
        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = currentUserService.UserId.Value,
            CompanyDetails = request.CompanyDetails,
            VerificationDocumentsUrl = request.VerificationDocumentsUrl
        };

        var applicationId = await mediator.Send(command);

        // Stream back a standardized 201 Created resource collection footprint mapping payload
        return Created(string.Empty, new { Id = applicationId });
    }
}
using AcademicGateway.Application.Features.Providers.Commands.SubmitApplication;
using AcademicGateway.Application.Features.TechSupportAccounts.Commands.CreateTechSupportAccount;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Provider")] // Guard: Restricts controller access solely to registered Provider corporate identities
public class ProvidersController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Submits a formal registration and compliance vetting application for verification.
    /// </summary>
    [HttpPost("applications")]
    public async Task<ActionResult<Guid>> SubmitApplication([FromBody] ApiSubmitApplicationRequest request)
    {
        // Safely extract the primary identity key from the current authorization token claims
        var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(providerId))
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
    [HttpPost("tech-support")]
    public async Task<ActionResult<Guid>> CreateTechSupport([FromBody] ApiCreateTechSupportRequest request)
    {
        var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(providerId))
        {
            return Unauthorized("Provider corporate security context could not be resolved from token metadata.");
        }

        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = providerId,
            Email = request.Email,
            Password = request.Password,
            FullName = request.FullName
        };

        var techAccountId = await mediator.Send(command);
        return Ok(techAccountId);
    }
}

// Separate binding contracts to securely isolate HTTP request inputs from deep Application layers
public record ApiSubmitApplicationRequest(string CompanyDetails, string VerificationDocumentsUrl);
public record ApiCreateTechSupportRequest(string Email, string Password, string FullName);
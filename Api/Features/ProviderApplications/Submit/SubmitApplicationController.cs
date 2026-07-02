using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProviderApplications.Commands.SubmitProviderApplication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGateway.Api.Features.ProviderApplications.Submit;

[Authorize(Roles = "Provider")]
[ApiController]
[Route("api/providers")]
public class SubmitApplicationController(
    ISender mediator,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpPost("applications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Guid>> SubmitApplication([FromBody] SubmitApplicationRequest request)
    {
        if (!currentUserService.UserId.HasValue)
        {
            return Unauthorized("Provider corporate security context could not be resolved.");
        }

        var command = new SubmitProviderApplicationCommand
        {
            ProviderId = currentUserService.UserId.Value,
            CompanyDetails = request.CompanyDetails,
            VerificationDocumentsUrl = request.VerificationDocumentsUrl
        };

        var applicationId = await mediator.Send(command);
        return Ok(applicationId);
    }
}

public record SubmitApplicationRequest(string CompanyDetails, string VerificationDocumentsUrl);
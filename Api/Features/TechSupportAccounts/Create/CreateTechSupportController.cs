using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.TechSupportAccounts.Commands.CreateTechSupportAccount;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGateway.Api.Features.TechSupportAccounts.Create;

[Authorize(Roles = "Provider")]
[ApiController]
[Route("api/providers")]
public class CreateTechSupportController(
    ISender mediator,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpPost("tech-support")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Guid>> CreateTechSupport([FromBody] CreateTechSupportRequest request)
    {
        if (!currentUserService.UserId.HasValue)
        {
            return Unauthorized("Provider corporate security context could not be resolved.");
        }

        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = currentUserService.UserId.Value,
            Email = request.Email,
            Password = request.Password
        };

        var techAccountId = await mediator.Send(command);
        return Ok(techAccountId);
    }
}

public record CreateTechSupportRequest(string Email, string Password);
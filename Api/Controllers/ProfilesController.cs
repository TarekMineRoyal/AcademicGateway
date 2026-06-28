using AcademicGateway.Application.Features.Users.Queries.GetProfessorProfile;
using AcademicGateway.Application.Features.Users.Queries.GetProviderProfile;
using AcademicGateway.Application.Features.Users.Queries.GetStudentProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProfilesController(ISender mediator) : ControllerBase
{
    [HttpGet("student")]
    public async Task<IActionResult> GetStudentProfile()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var result = await mediator.Send(new GetStudentProfileQuery(userId));
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpGet("professor")]
    public async Task<IActionResult> GetProfessorProfile()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var result = await mediator.Send(new GetProfessorProfileQuery(userId));
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpGet("provider")]
    public async Task<IActionResult> GetProviderProfile()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var result = await mediator.Send(new GetProviderProfileQuery(userId));
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    private string? GetCurrentUserId()
    {
        // Extracts the subject claim. Depending on standard framework mappings, 
        // the inbound 'sub' claim maps to ClaimTypes.NameIdentifier.
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
    }
}
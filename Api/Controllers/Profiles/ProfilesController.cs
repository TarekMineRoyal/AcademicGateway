using AcademicGateway.Application.Features.Professors.Queries.GetProfessorProfile;
using AcademicGateway.Application.Features.Providers.Queries.GetProviderProfile;
using AcademicGateway.Application.Features.Students.Queries.GetStudentProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Api.Controllers.Profiles;

/// <summary>
/// Provides access to user profile data. 
/// NOTE: This controller is intended to be decomposed into specific feature-based controllers in future iterations.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProfilesController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Retrieves the profile information for the authenticated student.
    /// </summary>
    [HttpGet("student")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentProfile()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await mediator.Send(new GetStudentProfileQuery(userId.Value));
        return Ok(result);
    }

    /// <summary>
    /// Retrieves the profile information for the authenticated professor.
    /// </summary>
    [HttpGet("professor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfessorProfile()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await mediator.Send(new GetProfessorProfileQuery(userId.Value));
        return Ok(result);
    }

    /// <summary>
    /// Retrieves the profile information for the authenticated provider.
    /// </summary>
    [HttpGet("provider")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProviderProfile()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await mediator.Send(new GetProviderProfileQuery(userId.Value));
        return Ok(result);
    }

    /// <summary>
    /// Safely extracts and parses the User ID as a Guid from the identity claims.
    /// </summary>
    private Guid? GetCurrentUserId()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;

        if (Guid.TryParse(userIdString, out var guid))
        {
            return guid;
        }

        return null;
    }
}
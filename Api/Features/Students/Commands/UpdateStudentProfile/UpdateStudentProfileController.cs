using AcademicGateway.Application.Features.Students.Commands.UpdateStudentProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.Students.Commands.UpdateStudentProfile;

/// <summary>
/// Presentation layer request schema for modifying a student's profile specifications.
/// </summary>
public record UpdateStudentProfileRequest(
    string FullName,
    int? GraduationYear,
    IReadOnlyCollection<Guid> MajorIds,
    IReadOnlyCollection<Guid> SpecialtyIds,
    IReadOnlyCollection<Guid> SkillIds,
    string? AboutMe = null);

/// <summary>
/// Single Action Controller endpoint for managing active student profile lifecycles and maintenance operations.
/// </summary>
[ApiController]
[Tags("Students")]
[Authorize(Roles = "Student")]
[Route("api/students")]
public class UpdateStudentProfileController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Updates the currently authenticated student's profile details, academic majors, specialties, and technical skills.
    /// </summary>
    /// <param name="request">The presentational request body envelope containing rewritten profile attributes and relational lookups.</param>
    /// <returns>A 204 No Content response indicating a successful atomic state synchronization.</returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateStudentProfileRequest request)
    {
        // Explicitly isolate public contract primitives from internal application query/command shapes
        var command = new UpdateStudentProfileCommand
        {
            FullName = request.FullName,
            GraduationYear = request.GraduationYear,
            MajorIds = request.MajorIds,
            SpecialtyIds = request.SpecialtyIds,
            SkillIds = request.SkillIds,
            AboutMe = request.AboutMe
        };

        await mediator.Send(command);

        // Returns a standard 204 No Content indicating successful resource modification with an empty body
        return NoContent();
    }
}
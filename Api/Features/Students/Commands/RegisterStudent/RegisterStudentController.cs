using AcademicGateway.Api.Common.Models;
using AcademicGateway.Application.Features.Students.Commands.RegisterStudent;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.Students.Commands.RegisterStudent;

/// <summary>
/// Presentation layer request schema for initiating a public student registration entry.
/// </summary>
public record RegisterStudentRequest(
    string Email,
    string Username,
    string Password,
    string FullName,
    int? GraduationYear,
    IReadOnlyCollection<Guid> MajorIds,
    IReadOnlyCollection<Guid> SpecialtyIds,
    IReadOnlyCollection<Guid> SkillIds,
    string? AboutMe = null);

/// <summary>
/// Single Action Controller endpoint for registering new students.
/// </summary>
[ApiController]
[Tags("Students")]
[AllowAnonymous]
[Route("api/students")]
public class RegisterStudentController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Registers a new student account and initializes their platform aggregate profile.
    /// </summary>
    /// <param name="request">The presentation request body envelope containing registration credentials and selection matrix rules.</param>
    /// <returns>A 201 Created response carrying a strongly-typed contract containing the unique identifier generated for the student.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResourceCreatedResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterStudentRequest request)
    {
        // Decouple internal application primitives by mapping explicitly from the presentation model
        var command = new RegisterStudentCommand
        {
            Email = request.Email,
            Username = request.Username,
            Password = request.Password,
            FullName = request.FullName,
            GraduationYear = request.GraduationYear,
            MajorIds = request.MajorIds,
            SpecialtyIds = request.SpecialtyIds,
            SkillIds = request.SkillIds,
            AboutMe = request.AboutMe
        };

        var studentId = await mediator.Send(command);

        // Returns a standardized strongly typed contract signaling successful resource creation
        return Created(string.Empty, new ResourceCreatedResponse(studentId));
    }
}
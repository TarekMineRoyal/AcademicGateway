using AcademicGateway.Api.Common.Models;
using AcademicGateway.Application.Features.Professors.Commands.RegisterProfessor;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.Professors.Commands.RegisterProfessor;

/// <summary>
/// Presentation layer request schema for registering a new institutional professor account.
/// </summary>
public record RegisterProfessorRequest(
    string Email,
    string Username,
    string Password,
    string FullName,
    string AcademicDepartment,
    string Rank,
    int MaxSupervisionCapacity,
    string? AboutMe = null);

/// <summary>
/// Endpoint for registering new institutional faculty members.
/// </summary>
[ApiController]
[Tags("Professors")]
[AllowAnonymous]
[Route("api/professors")]
public class RegisterProfessorController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Registers a new professor, provisions their identity security credentials, and creates their domain profile.
    /// </summary>
    /// <param name="request">The presentation request body envelope containing registration credentials and capacity allocations.</param>
    /// <returns>A 201 Created response carrying a strongly-typed contract containing the unique identifier generated for the professor.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResourceCreatedResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterProfessorRequest request)
    {
        // Explicitly map presentation request primitives to the inner MediatR command to protect the public contract boundary
        var command = new RegisterProfessorCommand
        {
            Email = request.Email,
            Username = request.Username,
            Password = request.Password,
            FullName = request.FullName,
            AcademicDepartment = request.AcademicDepartment,
            Rank = request.Rank,
            MaxSupervisionCapacity = request.MaxSupervisionCapacity,
            AboutMe = request.AboutMe
        };

        var professorId = await mediator.Send(command);

        // Returns a standardized strongly typed contract signaling successful resource creation
        return Created(string.Empty, new ResourceCreatedResponse(professorId));
    }
}
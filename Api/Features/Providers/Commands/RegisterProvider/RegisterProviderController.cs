using AcademicGateway.Api.Common.Models;
using AcademicGateway.Application.Features.Providers.Commands.RegisterProvider;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AcademicGateway.Api.Features.Providers.Commands.RegisterProvider;

/// <summary>
/// Presentation layer request schema for registering a new corporate industry provider account.
/// </summary>
public record RegisterProviderRequest(
    string Email,
    string Username,
    string Password,
    string CompanyName,
    string CompanyDescription,
    string? WebsiteUrl);

/// <summary>
/// Endpoint for registering new corporate industry providers.
/// </summary>
[ApiController]
[Tags("Providers")]
[AllowAnonymous]
[Route("api/providers")]
public class RegisterProviderController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Registers a new corporate provider account and initializes their platform aggregate profile.
    /// </summary>
    /// <param name="request">The presentation request body envelope containing registration credentials and corporate focus parameters.</param>
    /// <returns>A 201 Created response carrying a strongly-typed contract containing the unique identifier generated for the provider.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResourceCreatedResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterProviderRequest request)
    {
        // Explicitly isolate public contract primitives from internal application commands
        var command = new RegisterProviderCommand
        {
            Email = request.Email,
            Username = request.Username,
            Password = request.Password,
            CompanyName = request.CompanyName,
            CompanyDescription = request.CompanyDescription,
            WebsiteUrl = request.WebsiteUrl
        };

        var providerId = await mediator.Send(command);

        // Returns a standardized strongly typed contract signaling successful resource creation
        return Created(string.Empty, new ResourceCreatedResponse(providerId));
    }
}
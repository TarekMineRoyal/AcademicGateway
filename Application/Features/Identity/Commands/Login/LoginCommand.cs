using MediatR;

namespace AcademicGateway.Application.Features.Identity.Commands.Login;

/// <summary>
/// CQRS Command to authenticate user credentials against the centralized identity service.
/// Upon successful verification, generates a secure, cryptographically signed token stream.
/// </summary>
/// <param name="Email">The unique corporate or academic institutional email address tracking the identity credential.</param>
/// <param name="Password">The plain-text authentication secret provided by the applicant.</param>
public record LoginCommand(string Email, string Password) : IRequest<string>;
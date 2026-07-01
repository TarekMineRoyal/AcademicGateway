using AcademicGateway.Application.Common.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Auth.Commands.Login;

/// <summary>
/// Handles the execution of the <see cref="LoginCommand"/> to authenticate user credentials.
/// Verifies identity matching via the core security engine and manages access authorization tokens.
/// </summary>
public class LoginCommandHandler(IIdentityService identityService)
    : IRequestHandler<LoginCommand, string>
{
    /// <summary>
    /// Processes the user authentication lookup sequence.
    /// </summary>
    /// <param name="request">The incoming login transaction parameters containing credentials.</param>
    /// <param name="cancellationToken">Propagates notification that network operations should be canceled.</param>
    /// <returns>A cryptographically signed security token string confirming active verification status.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if credential verification fails or user matching records reject authentication.</exception>
    public async Task<string> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Attempt to authenticate the user using your identity service
        var token = await identityService.AuthenticateAsync(request.Email, request.Password);

        // 2. The Guard Clause
        // If the service returns null/empty because of a bad email or password, throw!
        if (string.IsNullOrWhiteSpace(token))
        {
            // Always use a generic message to prevent Username Enumeration Attacks
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // 3. Return the valid token
        return token;
    }
}
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGateway.Application.Common.Interfaces;

namespace AcademicGateway.Application.Features.Users.Commands.Login;

public class LoginCommandHandler(IIdentityService identityService)
    : IRequestHandler<LoginCommand, string> // Adjust return type if you use an AuthResponseDto
{
    public async Task<string> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Attempt to authenticate the user using your identity service
        var token = await identityService.AuthenticateAsync(request.Email, request.Password);

        // 2. The Guard Clause (This is the bug fix!)
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
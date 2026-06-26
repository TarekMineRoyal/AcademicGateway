using AcademicGateway.Application.Common.Interfaces;
using MediatR;

namespace AcademicGateway.Application.Features.Users.Commands.Login;

public class LoginCommandHandler(IIdentityService identityService) : IRequestHandler<LoginCommand, string?>
{
    public async Task<string?> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await identityService.AuthenticateAsync(request.Email, request.Password);
    }
}
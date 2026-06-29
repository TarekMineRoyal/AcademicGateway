using MediatR;

namespace AcademicGateway.Application.Features.Users.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<string>;
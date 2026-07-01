using MediatR;
using System;

namespace AcademicGateway.Application.Features.TechSupportAccounts.Commands.CreateTechSupportAccount;

public record CreateTechSupportAccountCommand : IRequest<Guid>
{
    // The authenticated Provider executing the creation
    public Guid ProviderId { get; init; }

    // Login credentials for the newly provisioned auxiliary account
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;

    // Profile details
    public string FullName { get; init; } = string.Empty;
}
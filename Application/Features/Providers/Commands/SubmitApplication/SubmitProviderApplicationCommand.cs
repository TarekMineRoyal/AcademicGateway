using MediatR;
using System;

namespace AcademicGateway.Application.Features.Providers.Commands.SubmitApplication;

public record SubmitProviderApplicationCommand : IRequest<Guid>
{
    public string ProviderId { get; init; } = string.Empty;
    public string CompanyDetails { get; init; } = string.Empty;
    public string VerificationDocumentsUrl { get; init; } = string.Empty;
}
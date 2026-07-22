using MediatR;
using System;

namespace AcademicGateway.Application.Features.ProviderApplications.Queries.GetProviderApplicationById;

/// <summary>
/// CQRS Query request to retrieve complete provider application details, credentials, 
/// attached documents, and evaluation history by ID for authorized reviewers.
/// </summary>
public record GetProviderApplicationByIdQuery : IRequest<ProviderApplicationDto?>
{
    /// <summary>
    /// Gets the unique lookup tracking identifier of the target ProviderApplication record.
    /// </summary>
    public Guid Id { get; init; }
}
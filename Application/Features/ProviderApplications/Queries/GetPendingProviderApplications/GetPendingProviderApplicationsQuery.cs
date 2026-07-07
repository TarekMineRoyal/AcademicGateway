using MediatR;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.ProviderApplications.Queries.GetPendingProviderApplications;

/// <summary>
/// CQRS query for retrieving the operational queue of provider onboarding applications currently awaiting evaluation.
/// Consumed exclusively by authorized administrative quality assurance reviewers to manage verification pipelines.
/// </summary>
public record GetPendingProviderApplicationsQuery : IRequest<IReadOnlyCollection<PendingProviderApplicationDto>>;
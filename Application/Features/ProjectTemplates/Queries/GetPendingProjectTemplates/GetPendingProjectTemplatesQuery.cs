using MediatR;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.ProjectTemplates.Queries.GetPendingProjectTemplates;

/// <summary>
/// CQRS query for retrieving the operational blueprint clearance queue of project templates currently awaiting review.
/// Consumed exclusively by authorized administrative quality assurance reviewers to inspect, comment on, and approve incoming industry templates.
/// </summary>
public record GetPendingProjectTemplatesQuery : IRequest<IReadOnlyCollection<PendingProjectTemplateDto>>;
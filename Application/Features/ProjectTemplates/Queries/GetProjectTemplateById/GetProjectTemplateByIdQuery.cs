using MediatR;
using System;

namespace AcademicGateway.Application.Features.ProjectTemplates.Queries.GetProjectTemplateById;

/// <summary>
/// CQRS Query to retrieve the complete structural configurations, validation nodes, 
/// and directed execution dependency constraints for a specific project template blueprint.
/// </summary>
public record GetProjectTemplateByIdQuery : IRequest<ProjectTemplateDetailDto>
{
    /// <summary>
    /// Gets the unique lookup tracking identifier of the targeted parent ProjectTemplate aggregate root.
    /// </summary>
    public Guid Id { get; init; }
}
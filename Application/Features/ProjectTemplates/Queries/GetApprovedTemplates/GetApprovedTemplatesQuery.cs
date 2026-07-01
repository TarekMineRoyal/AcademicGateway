using MediatR;
using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.ProjectTemplates.Queries.GetApprovedTemplates;

/// <summary>
/// CQRS Query to retrieve a filtered collection of approved, publicly available project template blueprints.
/// Commonly consumed by student matching platforms to discover verified placement options.
/// </summary>
public record GetApprovedTemplatesQuery : IRequest<IReadOnlyCollection<ApprovedTemplateDto>>
{
    /// <summary>
    /// Gets an optional filtering criterion targeting a specific technical skill requirement lookup identifier.
    /// When provided, restricts results strictly to project blueprints requiring this asset.
    /// </summary>
    public Guid? SkillId { get; init; }
}
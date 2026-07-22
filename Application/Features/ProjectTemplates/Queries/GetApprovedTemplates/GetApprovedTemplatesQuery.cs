using AcademicGateway.Application.Common.Models;
using MediatR;
using System;

namespace AcademicGateway.Application.Features.ProjectTemplates.Queries.GetApprovedTemplates;

/// <summary>
/// CQRS Query to retrieve a filtered collection of approved, publicly available project template blueprints.
/// Commonly consumed by student matching platforms to discover verified placement options.
/// </summary>
/// <param name="SkillId">An optional filtering criterion targeting a specific technical skill requirement lookup identifier.</param>
/// <param name="PageNumber">The 1-based index of the page to retrieve (default: 1).</param>
/// <param name="PageSize">The maximum number of items to retrieve per page (default: 10).</param>
public record GetApprovedTemplatesQuery(
    Guid? SkillId = null,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PaginatedResult<ApprovedTemplateDto>>;
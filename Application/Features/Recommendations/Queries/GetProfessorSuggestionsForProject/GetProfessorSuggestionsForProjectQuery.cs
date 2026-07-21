using System;
using System.Collections.Generic;
using AcademicGateway.Application.Features.Professors.Queries.SearchProfessors;
using MediatR;

namespace AcademicGateway.Application.Features.Recommendations.Queries.GetProfessorSuggestionsForProject;

/// <summary>
/// CQRS Query to retrieve vector-matched, AI-ranked faculty advisor suggestions for a given project template.
/// </summary>
public record GetProfessorSuggestionsForProjectQuery : IRequest<IReadOnlyCollection<ProfessorSearchResultDto>>
{
    /// <summary>
    /// Gets the unique identifier of the target project template blueprint.
    /// </summary>
    public Guid ProjectTemplateId { get; init; }

    /// <summary>
    /// Gets the maximum number of professor suggestions to return (default: 10).
    /// </summary>
    public int Limit { get; init; } = 10;
}
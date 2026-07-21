using System;
using System.Collections.Generic;
using AcademicGateway.Application.Features.Professors.Queries.SearchProfessors;
using MediatR;

namespace AcademicGateway.Application.Features.Recommendations.Queries.GetProfessorSuggestionsForProject;

/// <summary>
/// CQRS Query to retrieve vector-matched, AI-ranked faculty advisor suggestions for a project blueprint.
/// Accepts either an existing ProjectTemplateId to resolve context automatically or raw project context parameters.
/// </summary>
public record GetProfessorSuggestionsForProjectQuery : IRequest<IReadOnlyCollection<ProfessorSearchResultDto>>
{
    /// <summary>
    /// Gets the optional unique identifier of an existing project template blueprint to query context from.
    /// </summary>
    public Guid? TemplateId { get; init; }

    /// <summary>
    /// Gets the optional headline title of the project context.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets the optional detailed description of the project context.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the optional target academic major name string.
    /// </summary>
    public string? MajorName { get; init; }

    /// <summary>
    /// Gets the optional target academic specialty name string.
    /// </summary>
    public string? SpecialtyName { get; init; }

    /// <summary>
    /// Gets the optional list of required skill names.
    /// </summary>
    public List<string>? SkillNames { get; init; }

    /// <summary>
    /// Gets the maximum number of professor suggestions to return (default: 10).
    /// </summary>
    public int Limit { get; init; } = 10;
}
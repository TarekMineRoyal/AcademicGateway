using System.Collections.Generic;
using AcademicGateway.Application.Features.ProjectTemplates.Queries.GetApprovedTemplates;
using MediatR;

namespace AcademicGateway.Application.Features.Recommendations.Queries.GetProjectRecommendationsForStudent;

/// <summary>
/// CQRS Query to retrieve vector-matched, AI-ranked project template recommendations tailored for the authenticated student.
/// Evaluates student major, specialties, skills, and bio context to return ordered project blueprints.
/// </summary>
public record GetProjectRecommendationsForStudentQuery : IRequest<IReadOnlyCollection<ApprovedTemplateDto>>
{
    /// <summary>
    /// Gets the maximum number of recommended project templates to return (default: 10).
    /// </summary>
    public int Limit { get; init; } = 10;
}
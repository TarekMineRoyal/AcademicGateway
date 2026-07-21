using System.Collections.Generic;
using AcademicGateway.Application.Features.Skills.Queries.GetSkills;
using MediatR;

namespace AcademicGateway.Application.Features.Recommendations.Queries.GetSkillRecommendationsForStudent;

/// <summary>
/// CQRS Query to retrieve vector-matched, AI-ranked adjacent skill recommendations for the authenticated student.
/// Evaluates current student profile competencies, major, and bio to suggest technical growth areas.
/// </summary>
public record GetSkillRecommendationsForStudentQuery : IRequest<IReadOnlyCollection<SkillDto>>
{
    /// <summary>
    /// Gets the maximum number of recommended skill competencies to return (default: 10).
    /// </summary>
    public int Limit { get; init; } = 10;
}
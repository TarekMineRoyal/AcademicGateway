using AcademicGateway.Application.Features.Professors.Queries.GetProfessorProfile;
using MediatR;
using System;

namespace AcademicGateway.Application.Features.Professors.Queries.GetProfessorProfile;

/// <summary>
/// CQRS Query to retrieve the full bio profile, departmental mappings, 
/// and active research and supervision analytics for a specific institutional professor.
/// </summary>
/// <param name="ProfessorId">The unique global identifier of the professor profile, mapping 1:1 to their security identity context.</param>
public record GetProfessorProfileQuery(Guid ProfessorId) : IRequest<ProfessorProfileDto>;
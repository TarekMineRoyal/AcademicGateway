using System;
using MediatR;

namespace AcademicGateway.Application.Features.Professors.Queries.GetPublicProfessorProfile;

/// <summary>
/// CQRS Query for retrieving a professor's public profile by their unique identifier without tenancy restrictions.
/// </summary>
/// <param name="ProfessorId">The unique identity tracker of the target faculty profile.</param>
public record GetPublicProfessorProfileQuery(Guid ProfessorId) : IRequest<GetPublicProfessorProfileQueryDto?>;
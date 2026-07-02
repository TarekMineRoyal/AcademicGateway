using MediatR;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.Curriculum.Queries.GetMajorsWithSpecialties;

/// <summary>
/// CQRS Query to fetch all active academic majors along with their nested sub-specialties.
/// This lookup query is open and commonly consumed during student registration and profile setup workflows.
/// </summary>
public record GetMajorsWithSpecialtiesQuery : IRequest<IReadOnlyCollection<MajorDto>>;
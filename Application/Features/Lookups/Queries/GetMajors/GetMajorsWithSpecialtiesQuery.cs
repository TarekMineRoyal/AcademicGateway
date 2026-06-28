using MediatR;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.Lookups.Queries.GetMajors;

public record GetMajorsWithSpecialtiesQuery : IRequest<List<MajorDto>>;
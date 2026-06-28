using MediatR;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.Lookups.Queries.GetSkills;

public record GetSkillsQuery : IRequest<List<SkillDto>>;
using MediatR;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.Lookups.Queries.GetSkills;

/// <summary>
/// CQRS Query to fetch all active technical capabilities, professional competencies, and skills configured within the system.
/// This open global lookup is commonly consumed when populating skills inventories during student profile setups.
/// </summary>
public record GetSkillsQuery : IRequest<IReadOnlyCollection<SkillDto>>;
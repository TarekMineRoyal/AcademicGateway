using MediatR;
using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.Skills.Queries.GetUserSkills;

/// <summary>
/// CQRS Query to fetch the specialized technical capabilities and professional competencies assigned to a specific user profile.
/// Filters the relational join tracking matrix records using the provided unique system user identity tracking reference.
/// </summary>
/// <param name="UserId">The unique Identity tracking primary key assigned onto the target user profile.</param>
public record GetUserSkillsQuery(Guid UserId) : IRequest<IReadOnlyCollection<UserSkillDto>>;
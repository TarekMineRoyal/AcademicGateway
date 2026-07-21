using System;
using AcademicGateway.Domain.Common;

namespace AcademicGateway.Domain.Skills.Events;

/// <summary>
/// Domain event raised whenever a new Skill entity is created in the system.
/// </summary>
public record SkillCreatedEvent(Guid SkillId) : IDomainEvent;
using System;
using AcademicGateway.Domain.Common;

namespace AcademicGateway.Domain.Skills.Events;

/// <summary>
/// Domain event raised whenever a Skill entity is deleted from the system.
/// </summary>
public record SkillDeletedEvent(Guid SkillId) : IDomainEvent;
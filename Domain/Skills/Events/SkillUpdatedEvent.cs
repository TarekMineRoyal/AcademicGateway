using System;
using AcademicGateway.Domain.Common;

namespace AcademicGateway.Domain.Skills.Events;

/// <summary>
/// Domain event raised whenever an existing Skill's descriptive details are updated.
/// </summary>
public record SkillUpdatedEvent(Guid SkillId) : IDomainEvent;
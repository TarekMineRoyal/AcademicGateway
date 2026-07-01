using MediatR;
using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.CreateTemplate;

public record CreateProjectTemplateCommand : IRequest<Guid>
{
    public Guid ProviderId { get; init; } // Resolved from the authenticated corporate provider session token
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int ExpectedDurationWeeks { get; init; }
    public List<Guid> SkillIds { get; init; } = new(); // The required skills linked via the explicit join entity
}
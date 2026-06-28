using MediatR;
using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.ProjectTemplates.Queries.GetApprovedTemplates;

public record GetApprovedTemplatesQuery : IRequest<List<ApprovedTemplateDto>>
{
    // Filter by maximum project duration length (e.g., up to 4 weeks, up to 12 weeks)
    public int? MaxDurationWeeks { get; init; }

    // Filter by a specific required technical skill identifier
    public Guid? SkillId { get; init; }
}
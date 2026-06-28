using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.ProjectTemplates.Queries.GetApprovedTemplates;

public record ApprovedTemplateDto
{
    public Guid Id { get; init; }
    public string ProviderId { get; init; } = string.Empty;
    public string ProviderCompanyName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int ExpectedDurationWeeks { get; init; }
    public List<TemplateSkillDto> Skills { get; init; } = new();
}

public record TemplateSkillDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
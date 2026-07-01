using MediatR;
using System;

namespace AcademicGateway.Application.Features.ProjectTemplates.Commands.ReviewTemplate;

public record ReviewProjectTemplateCommand : IRequest
{
    public Guid TemplateId { get; init; }
    public Guid ReviewerIdentityUserId { get; init; } // Extracted from the authenticated Reviewer token
    public bool IsApproved { get; init; }
    public string? RejectionReason { get; init; }
}
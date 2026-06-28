using MediatR;
using System;

namespace AcademicGateway.Application.Features.Reviewers.Commands.ReviewApplication;

public record ReviewProviderApplicationCommand : IRequest
{
    public Guid ApplicationId { get; init; }
    public string ReviewerIdentityUserId { get; init; } = string.Empty; // Resolved from the authenticated HTTP context token
    public bool IsApproved { get; init; }
    public string? RejectionReason { get; init; }
}
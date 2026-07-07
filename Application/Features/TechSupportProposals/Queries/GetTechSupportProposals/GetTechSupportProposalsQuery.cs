using MediatR;
using System;
using System.Collections.Generic;

namespace AcademicGateway.Application.Features.TechSupportProposals.Queries.GetTechSupportProposals;

/// <summary>
/// CQRS query request contract targeting corporate industry mentorship proposals.
/// Requests the active assistance offers tied to a specific project workspace instance.
/// </summary>
public class GetTechSupportProposalsQuery : IRequest<IReadOnlyCollection<TechSupportProposalDto>>
{
    /// <summary>
    /// Gets or sets the live project workspace aggregate root's unique lookup identifier key.
    /// </summary>
    public Guid ProjectInstanceId { get; set; }
}
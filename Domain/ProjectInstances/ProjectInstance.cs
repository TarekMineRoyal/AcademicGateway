using System;
using System.Collections.Generic;
using System.Linq;
using AcademicGateway.Domain.Common;
using AcademicGateway.Domain.ProjectInstances.Enums;
using AcademicGateway.Domain.ProjectInstances.Events;
using AcademicGateway.Domain.ProjectInstances.Exceptions;
using AcademicGateway.Domain.Students;
using AcademicGateway.Domain.Professors;

namespace AcademicGateway.Domain.ProjectInstances;

/// <summary>
/// Represents an active, running runtime instance of a project workspace initialized by a student.
/// Acts as the Aggregate Root for the execution subdomain, managing matching, mentorship, and milestones.
/// </summary>
public class ProjectInstance : BaseEntity
{
    private readonly List<ProjectInstanceSkill> _snapshotSkills = new();
    private readonly List<SupervisionRequest> _supervisionRequests = new();
    private readonly List<TechSupportProposal> _techSupportProposals = new();

    /// <summary>
    /// Gets the unique tracking identifier for the live project workspace.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the identifier code mapping back to the owner student account.
    /// </summary>
    public Guid StudentId { get; private set; }

    /// <summary>
    /// Gets the identifier code mapping to the assigned faculty mentor, if any supervisor has signed on.
    /// </summary>
    public Guid? SupervisorId { get; private set; }

    /// <summary>
    /// Gets the identifier of the source template blueprint this instance was spawned from.
    /// </summary>
    public Guid TemplateId { get; private set; }

    /// <summary>
    /// Gets the identifier of the creating corporate provider, snapshotted to grant 
    /// tracking visibility to industry sponsors independently of template modifications.
    /// </summary>
    public Guid ProviderId { get; private set; }

    /// <summary>
    /// Gets the historical snapshot copy of the project's headline title.
    /// </summary>
    public string TitleSnapshot { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the historical snapshot copy of the project's core requirements overview text.
    /// </summary>
    public string DescriptionSnapshot { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the current state within the active platform matching and execution lifecycle.
    /// </summary>
    public ProjectInstanceStatus Status { get; private set; }

    /// <summary>
    /// Gets the point-in-time timestamp indicating when this live workspace was initialized.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Gets the strict administrative deadline capping active work execution, set by the supervisor.
    /// </summary>
    public DateTime? EndDate { get; private set; }

    /// <summary>
    /// Gets the navigation property for the owner student profile.
    /// </summary>
    public Student Student { get; private set; } = null!;

    /// <summary>
    /// Gets the navigation property for the co-managing academic faculty mentor.
    /// </summary>
    public Professor? Supervisor { get; private set; }

    /// <summary>
    /// Gets the read-only tracking collection of point-in-time technical skills mapped to this instance workspace.
    /// </summary>
    public IReadOnlyCollection<ProjectInstanceSkill> SnapshotSkills => _snapshotSkills.AsReadOnly();

    /// <summary>
    /// Gets the read-only historical tracking loop of academic supervision invitations.
    /// </summary>
    public IReadOnlyCollection<SupervisionRequest> SupervisionRequests => _supervisionRequests.AsReadOnly();

    /// <summary>
    /// Gets the read-only historical tracking loop of corporate industry mentorship proposals.
    /// </summary>
    public IReadOnlyCollection<TechSupportProposal> TechSupportProposals => _techSupportProposals.AsReadOnly();

    /// <summary>
    /// EF Core constructor requirement. Prevents bypass of standard domain constraints during persistence hydration.
    /// </summary>
    private ProjectInstance()
    {
    }

    /// <summary>
    /// Initializes a new instance of a running project workspace aggregate root.
    /// Intended for use primarily by the Domain Factory Method pattern implementation.
    /// </summary>
    internal ProjectInstance(
        Guid studentId,
        Guid templateId,
        Guid providerId,
        string titleSnapshot,
        string descriptionSnapshot,
        ProjectInstanceStatus initialStatus,
        DateTime createdAt,
        Guid? initialRequestedProfessorId,
        IEnumerable<Guid> skillIds)
    {
        Id = Guid.NewGuid();
        StudentId = studentId;
        TemplateId = templateId;
        ProviderId = providerId;
        TitleSnapshot = titleSnapshot.Trim();
        DescriptionSnapshot = descriptionSnapshot.Trim();
        Status = initialStatus;
        CreatedAt = createdAt;

        foreach (var skillId in skillIds)
        {
            _snapshotSkills.Add(new ProjectInstanceSkill(Id, skillId));
        }

        // Invariant: If a professor was selected at startup, seed the initial pending request record immediately
        if (initialRequestedProfessorId.HasValue && initialRequestedProfessorId.Value != Guid.Empty)
        {
            var initialRequest = new SupervisionRequest(
                Id,
                initialRequestedProfessorId.Value,
                "Initial matchmaking selection assigned during workspace blueprint instantiation.",
                createdAt);
            _supervisionRequests.Add(initialRequest);
        }

        AddDomainEvent(new ProjectInstanceStartedEvent(Id, TemplateId, StudentId, initialRequestedProfessorId));
    }

    // =========================================================================
    // STATE MACHINE TRANSITION METHODS & BUSINESS INVARIANTS
    // =========================================================================

    /// <summary>
    /// Issues a new matchmaking request to an academic supervisor. 
    /// Can be initiated mid-project if the student started solo.
    /// </summary>
    public void SubmitSupervisionRequest(Guid professorId, string pitchText, DateTime utcNow)
    {
        if (Status == ProjectInstanceStatus.Concluded || Status == ProjectInstanceStatus.Canceled)
        {
            throw new InvalidProjectInstanceTransitionException($"Cannot request supervision. The project workspace is currently '{Status}'.");
        }

        if (SupervisorId.HasValue)
        {
            throw new InvalidProjectInstanceTransitionException("This project instance is already bound to an active academic supervisor.");
        }

        // Invariant: Prevent duplicate active pending requests to protect faculty review backlogs
        if (_supervisionRequests.Any(r => r.Status == SupervisionRequestStatus.Pending))
        {
            throw new InvalidProjectInstanceTransitionException("An active supervision request is already pending review for this project workspace.");
        }

        var request = new SupervisionRequest(Id, professorId, pitchText, utcNow);
        _supervisionRequests.Add(request);

        AddDomainEvent(new SupervisionRequestCreatedEvent(request.Id, Id, professorId, request.PitchText));
    }

    /// <summary>
    /// Processes a professor's evaluation review for a outstanding pending supervision request.
    /// </summary>
    public void ReviewSupervisionRequest(Guid requestId, bool accept, string? rejectionReason, DateTime utcNow)
    {
        var request = _supervisionRequests.FirstOrDefault(r => r.Id == requestId);
        if (request == null)
        {
            throw new InvalidProjectInstanceTransitionException($"Supervision request with ID '{requestId}' was not found in this aggregate scope.");
        }

        if (request.Status != SupervisionRequestStatus.Pending)
        {
            throw new InvalidSupervisionRequestTransitionException(request.Status);
        }

        if (accept)
        {
            request.Status = SupervisionRequestStatus.Accepted;
            SupervisorId = request.ProfessorId;

            // If the project was paused waiting for onboarding validation, wake it up
            if (Status == ProjectInstanceStatus.AwaitingSupervision)
            {
                Status = ProjectInstanceStatus.Active;
            }

            // Decline any other historically lingering proposals to maintain single-supervisor isolation
            foreach (var pendingRequest in _supervisionRequests.Where(r => r.Id != requestId && r.Status == SupervisionRequestStatus.Pending))
            {
                pendingRequest.Status = SupervisionRequestStatus.Rejected;
                pendingRequest.RejectionReason = "System auto-rejection due to separate academic match completion.";
            }

            AddDomainEvent(new SupervisionRequestAcceptedEvent(request.Id, Id, request.ProfessorId));
        }
        else
        {
            request.Status = SupervisionRequestStatus.Rejected;
            request.RejectionReason = string.IsNullOrWhiteSpace(rejectionReason) ? "Declined by faculty." : rejectionReason.Trim();

            AddDomainEvent(new SupervisionRequestRejectedEvent(request.Id, Id, request.ProfessorId, request.RejectionReason));
        }
    }

    /// <summary>
    /// Explicitly transitions an onboarding paused project into an active solo configuration 
    /// if a requested matching failed or the student changes their mind.
    /// </summary>
    public void TransitionToSolo()
    {
        if (Status != ProjectInstanceStatus.AwaitingSupervision)
        {
            throw new InvalidProjectInstanceTransitionException($"Cannot pivot to solo execution. Instance must be 'AwaitingSupervision', but is currently '{Status}'.");
        }

        Status = ProjectInstanceStatus.Active;
        SupervisorId = null;

        // Cleanly clear out any outstanding pending match requests
        foreach (var pendingRequest in _supervisionRequests.Where(r => r.Status == SupervisionRequestStatus.Pending))
        {
            pendingRequest.Status = SupervisionRequestStatus.Rejected;
            pendingRequest.RejectionReason = "Withdrawn by student shifting to a solo project execution track.";
            AddDomainEvent(new SupervisionRequestRejectedEvent(pendingRequest.Id, Id, pendingRequest.ProfessorId, pendingRequest.RejectionReason));
        }
    }

    /// <summary>
    /// Registers a corporate sponsor's proposal to hook a technical support account into the student workspace board.
    /// </summary>
    public void ProposeTechSupport(Guid techSupportAccountId, DateTime utcNow)
    {
        if (Status != ProjectInstanceStatus.Active)
        {
            throw new InvalidProjectInstanceTransitionException($"Corporate mentors can only be attached to active running instances. Current status: '{Status}'.");
        }

        // Invariant: Prevent redundant active pending proposal offerings for the same support engineer account
        if (_techSupportProposals.Any(p => p.TechSupportAccountId == techSupportAccountId && p.Status == TechSupportProposalStatus.Pending))
        {
            throw new InvalidProjectInstanceTransitionException("A pending corporate assistance offer for this specific tech support account is already awaiting student feedback.");
        }

        var proposal = new TechSupportProposal(Id, techSupportAccountId, utcNow);
        _techSupportProposals.Add(proposal);

        AddDomainEvent(new TechSupportProposalCreatedEvent(proposal.Id, Id, techSupportAccountId));
    }

    /// <summary>
    /// Processes the student's evaluation decision over a corporate mentor assistance offer.
    /// </summary>
    public void ReviewTechSupportProposal(Guid proposalId, bool accept, string? rejectionReason)
    {
        var proposal = _techSupportProposals.FirstOrDefault(p => p.Id == proposalId);
        if (proposal == null)
        {
            throw new InvalidProjectInstanceTransitionException($"Corporate proposal tracking key '{proposalId}' was not found in this workspace scope.");
        }

        if (proposal.Status != TechSupportProposalStatus.Pending)
        {
            throw new InvalidTechSupportProposalTransitionException(proposal.Status);
        }

        if (accept)
        {
            proposal.Status = TechSupportProposalStatus.Accepted;
            AddDomainEvent(new TechSupportProposalAcceptedEvent(proposal.Id, Id, proposal.TechSupportAccountId));
        }
        else
        {
            proposal.Status = TechSupportProposalStatus.Rejected;
            proposal.RejectionReason = string.IsNullOrWhiteSpace(rejectionReason) ? "Declined by student workspace owner." : rejectionReason.Trim();
            AddDomainEvent(new TechSupportProposalRejectedEvent(proposal.Id, Id, proposal.TechSupportAccountId, proposal.RejectionReason));
        }
    }

    /// <summary>
    /// Sets an administrative project deadline capping running execution tasks. Restructured to supervisor authority.
    /// </summary>
    public void SetProjectEndDate(DateTime endDate, Guid executingProfessorId)
    {
        if (Status != ProjectInstanceStatus.Active)
        {
            throw new InvalidProjectInstanceTransitionException("Deadlines can only be assigned to live, active workspace channels.");
        }

        if (!SupervisorId.HasValue || SupervisorId.Value != executingProfessorId)
        {
            throw new InvalidProjectInstanceTransitionException("Access Denied: Only the assigned academic supervisor can manage timeline limitations on this instance.");
        }

        if (endDate <= DateTime.UtcNow)
        {
            throw new InvalidProjectInstanceTransitionException("The operational end boundary date must exist in the future.");
        }

        EndDate = endDate;
    }

    /// <summary>
    /// Concludes active work execution on the workspace, freezing contents for evaluation.
    /// </summary>
    public void ConcludeProject()
    {
        if (Status != ProjectInstanceStatus.Active && Status != ProjectInstanceStatus.AwaitingSupervision)
        {
            throw new InvalidProjectInstanceTransitionException(Status, ProjectInstanceStatus.Concluded);
        }

        Status = ProjectInstanceStatus.Concluded;
    }

    /// <summary>
    /// Terminate execution parameters prematurely, canceling the active workspace loop.
    /// </summary>
    public void CancelProject()
    {
        if (Status == ProjectInstanceStatus.Canceled || Status == ProjectInstanceStatus.Concluded)
        {
            throw new InvalidProjectInstanceTransitionException($"Cannot cancel a workspace that is already closed out. Status: '{Status}'.");
        }

        Status = ProjectInstanceStatus.Canceled;
    }
}
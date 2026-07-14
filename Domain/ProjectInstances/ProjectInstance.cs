using System;
using System.Collections.Generic;
using System.Linq;
using AcademicGateway.Domain.Common;
using AcademicGateway.Domain.Common.Enums;
using AcademicGateway.Domain.ProjectInstances.Enums;
using AcademicGateway.Domain.ProjectInstances.Events;
using AcademicGateway.Domain.ProjectInstances.Exceptions;
using AcademicGateway.Domain.ProjectInstances.Grading;
using AcademicGateway.Domain.Students;
using AcademicGateway.Domain.Professors;

namespace AcademicGateway.Domain.ProjectInstances;

/// <summary>
/// Represents an active, running runtime instance of a project workspace initialized by a student.
/// Acts as the Aggregate Root for the execution subdomain, managing matching, mentorship, milestone timelines, comments, and grading evaluation tracking loops.
/// </summary>
public class ProjectInstance : BaseEntity
{
    private readonly List<ProjectInstanceSkill> _snapshotSkills = new();
    private readonly List<SupervisionRequest> _supervisionRequests = new();
    private readonly List<TechSupportProposal> _techSupportProposals = new();
    private readonly List<LocalMilestone> _localMilestones = new();

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
    /// Gets the final numerical score value awarded to the project aggregate as a whole upon completion.
    /// </summary>
    public decimal? OverallGrade { get; private set; }

    /// <summary>
    /// Gets the timestamp tracking when the project aggregate's final evaluation was certified.
    /// </summary>
    public DateTime? ProjectGradedAt { get; private set; }

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
    /// Exposes the execution graph milestone items linked to this live workspace channel.
    /// </summary>
    public IReadOnlyCollection<LocalMilestone> LocalMilestones => _localMilestones.AsReadOnly();

    /// <summary>
    /// Evaluates whether the workspace WBS structures (milestones sum to 100% and internal tasks within all milestones sum to 100%) are structurally healthy.
    /// </summary>
    public bool IsWbsBalanced => _localMilestones.Sum(m => m.WbsWeight) == 100m && _localMilestones.All(m => m.IsWbsBalanced);

    /// <summary>
    /// Computes the macro-level rolled-up progress metric for the workspace. Returns null if structural integrity checks are unbalanced.
    /// </summary>
    public decimal? CompletionPercentage
    {
        get
        {
            if (!IsWbsBalanced)
            {
                return null;
            }

            decimal accumulatedProgress = 0m;
            foreach (var milestone in _localMilestones)
            {
                decimal milestoneTaskCompletionSum = 0m;
                foreach (var task in milestone.LocalTasks)
                {
                    if (task.Status == LocalTaskStatus.Submitted || task.Status == LocalTaskStatus.Graded)
                    {
                        milestoneTaskCompletionSum += task.Weight;
                    }
                }

                // Append the fraction of this milestone's completion into the absolute project scope allocation metric
                accumulatedProgress += (milestone.WbsWeight * (milestoneTaskCompletionSum / 100m));
            }

            return accumulatedProgress;
        }
    }

    /// <summary>
    /// EF Core constructor requirement. Prevents bypass of standard domain constraints during persistence hydration.
    /// </summary>
    private ProjectInstance()
    {
    }

    /// <summary>
    /// Initializes a new instance of a running project workspace aggregate root.
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

    /// <summary>
    /// Internal endpoint used exclusively by the <see cref="LocalMilestoneFactory"/> domain service 
    /// to seed the isolated cloned snapshot milestone records into the aggregate boundary collection.
    /// </summary>
    internal void SeedClonedMilestones(IEnumerable<LocalMilestone> milestones)
    {
        if (_localMilestones.Any())
        {
            throw new InvalidOperationException("The execution graph milestones for this project instance have already been populated.");
        }

        _localMilestones.AddRange(milestones);
    }

    // =========================================================================
    // STUDENT TIMELINE PLANNING & WORK EXECUTION MECHANICS
    // =========================================================================

    /// <summary>
    /// Updates the scheduling tracking timeline variables for an internal milestone execution leg,
    /// enforcing DAG scheduling dependency restrictions across the aggregate graph network.
    /// </summary>
    public void UpdateMilestoneTimeline(Guid milestoneId, DateTime startDate, DateTime endDate)
    {
        if (Status == ProjectInstanceStatus.Concluded || Status == ProjectInstanceStatus.Canceled)
        {
            throw new InvalidProjectInstanceTransitionException($"Cannot alter milestone deadlines. The workspace channel is '{Status}'.");
        }

        var targetMilestone = _localMilestones.FirstOrDefault(m => m.Id == milestoneId);
        if (targetMilestone == null)
        {
            throw new KeyNotFoundException($"Local Milestone with ID '{milestoneId}' was not found inside this project instance context.");
        }

        if (endDate <= startDate)
        {
            throw new InvalidOperationException("Invariant Violation: Scheduled End Date must be strictly later than Scheduled Start Date.");
        }

        var originalStartDate = targetMilestone.ScheduledStartDate;
        var originalEndDate = targetMilestone.ScheduledEndDate;

        targetMilestone.ScheduledStartDate = startDate;
        targetMilestone.ScheduledEndDate = endDate;

        try
        {
            EnsureTimelineDependenciesAreValid();
        }
        catch (InvalidOperationException)
        {
            targetMilestone.ScheduledStartDate = originalStartDate;
            targetMilestone.ScheduledEndDate = originalEndDate;
            throw;
        }

        targetMilestone.UpdateStatusFromTasks();
    }

    /// <summary>
    /// Orchestrates a student deliverable submission for an internal nested task, routing the payload 
    /// down through the specific milestone entity node while keeping child collections encapsulated.
    /// </summary>
    public void SubmitTaskDeliverable(Guid milestoneId, Guid taskId, string submissionPayload, DateTime utcNow)
    {
        if (Status == ProjectInstanceStatus.Concluded || Status == ProjectInstanceStatus.Canceled)
        {
            throw new InvalidProjectInstanceTransitionException($"Cannot submit deliverable. The project workspace is currently closed out as '{Status}'.");
        }

        var targetMilestone = _localMilestones.FirstOrDefault(m => m.Id == milestoneId);
        if (targetMilestone == null)
        {
            throw new KeyNotFoundException($"Local Milestone with ID '{milestoneId}' was not found inside this project instance context.");
        }

        targetMilestone.SubmitTaskDeliverable(taskId, submissionPayload, utcNow);
    }

    // =========================================================================
    // AGGREGATE ROUTER FOR DISCUSSION COMMENTS
    // =========================================================================

    /// <summary>
    /// Appends a collaboration comment to an internal milestone execution path. 
    /// Restricts inputs based on current workspace tracking lifecycle locks.
    /// </summary>
    public void AddMilestoneComment(Guid milestoneId, Guid authorId, string authorIdentitySnapshot, string content, DateTime utcNow)
    {
        if (Status == ProjectInstanceStatus.Concluded || Status == ProjectInstanceStatus.Canceled)
        {
            throw new InvalidProjectInstanceTransitionException($"Cannot post message commentary. The project workspace is currently '{Status}'.");
        }

        var targetMilestone = _localMilestones.FirstOrDefault(m => m.Id == milestoneId);
        if (targetMilestone == null)
        {
            throw new KeyNotFoundException($"Local Milestone with ID '{milestoneId}' was not found inside this project instance context boundary.");
        }

        targetMilestone.AddComment(authorId, authorIdentitySnapshot, content, utcNow);
    }

    // =========================================================================
    // GRADING & EVALUATION ENGINE (PROFESSOR OPTIONAL ADJUSTMENTS)
    // =========================================================================

    /// <summary>
    /// Orchestrates the evaluation transaction loop for a target nested task submission.
    /// Accommodates optional professor tracks by allowing students to self-certify/evaluate if no supervisor is bound.
    /// </summary>
    /// <param name="milestoneId">The unique tracking identifier of the target child milestone node.</param>
    /// <param name="taskId">The unique tracking identifier of the target child task node.</param>
    /// <param name="grade">The numerical score value awarded to the deliverable push.</param>
    /// <param name="feedback">Optional critique commentaries logged by the evaluator.</param>
    /// <param name="gradingStrategy">The concrete domain strategy algorithm governing evaluation rules.</param>
    /// <param name="utcNow">The current synchronized system timestamp execution coordinate.</param>
    /// <param name="executingUserId">The tracking identifier of the user processing the evaluation check.</param>
    /// <exception cref="InvalidProjectInstanceTransitionException">Thrown if identity checks or execution states fail constraints.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if the target milestone cannot be resolved.</exception>
    public void EvaluateTaskSubmission(
        Guid milestoneId,
        Guid taskId,
        decimal grade,
        string? feedback,
        IGradingStrategy gradingStrategy,
        DateTime utcNow,
        Guid executingUserId)
    {
        if (Status != ProjectInstanceStatus.Active)
        {
            throw new InvalidProjectInstanceTransitionException($"Evaluation Denied: Task scores can only be assigned to active running instances. Current status: '{Status}'.");
        }

        if (SupervisorId.HasValue && SupervisorId.Value != executingUserId)
        {
            throw new InvalidProjectInstanceTransitionException("Access Denied: Only the assigned academic supervisor possesses authority to grade submissions on this project workspace.");
        }
        if (!SupervisorId.HasValue && StudentId != executingUserId)
        {
            throw new InvalidProjectInstanceTransitionException("Access Denied: In a solo un-supervised track, only the owner student can process task evaluations.");
        }

        var targetMilestone = _localMilestones.FirstOrDefault(m => m.Id == milestoneId);
        if (targetMilestone == null)
        {
            throw new KeyNotFoundException($"Local Milestone with ID '{milestoneId}' was not found inside this project instance context boundary.");
        }

        targetMilestone.EvaluateTaskSubmission(taskId, grade, feedback, gradingStrategy, utcNow);
    }

    /// <summary>
    /// Evaluates and locks in the final macro-level aggregate grade score for the entire project workspace.
    /// Allows the owner student or the assigned supervisor to execute final closure loops.
    /// </summary>
    /// <param name="gradingStrategy">The concrete domain strategy algorithm governing the project workspace.</param>
    /// <param name="utcNow">The current synchronized system timestamp execution coordinate.</param>
    /// <param name="executingUserId">The tracking identifier of the user triggering final aggregate calculations.</param>
    /// <exception cref="InvalidProjectInstanceTransitionException">Thrown if workspace track states or authority checks fail constraints.</exception>
    public void FinalizeProjectGrade(IGradingStrategy gradingStrategy, DateTime utcNow, Guid executingUserId)
    {
        if (Status != ProjectInstanceStatus.Concluded)
        {
            throw new InvalidProjectInstanceTransitionException(
                $"Finalization Denied: Project grade scoring can only occur on workspaces marked as 'Concluded'. Current status: '{Status}'.");
        }

        bool isAuthorized = executingUserId == StudentId || (SupervisorId.HasValue && SupervisorId.Value == executingUserId);
        if (!isAuthorized)
        {
            throw new InvalidProjectInstanceTransitionException("Access Denied: You do not possess structural authority to finalize calculations on this project workspace.");
        }

        if (gradingStrategy == null)
        {
            throw new ArgumentNullException(nameof(gradingStrategy), "Final calculation processing requires a valid grading strategy reference.");
        }

        var computedFinalGrade = gradingStrategy.CalculateFinalProjectGrade(this._localMilestones);

        OverallGrade = computedFinalGrade;
        ProjectGradedAt = utcNow;
    }

    /// <summary>
    /// Iterates across the entire live milestone graph network collection, verifying that no 
    /// child node dates violate chronological dependency sequencing restrictions.
    /// </summary>
    private void EnsureTimelineDependenciesAreValid()
    {
        foreach (var milestone in _localMilestones)
        {
            if (!milestone.ScheduledStartDate.HasValue)
            {
                continue;
            }

            foreach (var dependency in milestone.InboundDependencies)
            {
                var predecessor = _localMilestones.First(m => m.Id == dependency.PredecessorId);

                if (dependency.Type == DependencyType.FinishToStart)
                {
                    if (predecessor.ScheduledEndDate.HasValue && milestone.ScheduledStartDate.Value < predecessor.ScheduledEndDate.Value)
                    {
                        throw new InvalidOperationException(
                            $"Timeline Constraint Conflict: Milestone '{milestone.TitleSnapshot}' is scheduled to start on " +
                            $"{milestone.ScheduledStartDate.Value:yyyy-MM-dd}, which violates a Finish-To-Start dependency on " +
                            $"Prerequisite '{predecessor.TitleSnapshot}' ending on {predecessor.ScheduledEndDate.Value:yyyy-MM-dd}.");
                    }
                }
                else if (dependency.Type == DependencyType.StartToStart)
                {
                    if (predecessor.ScheduledStartDate.HasValue && milestone.ScheduledStartDate.Value < predecessor.ScheduledStartDate.Value)
                    {
                        throw new InvalidOperationException(
                            $"Timeline Constraint Conflict: Milestone '{milestone.TitleSnapshot}' is scheduled to start on " +
                            $"{milestone.ScheduledStartDate.Value:yyyy-MM-dd}, which violates a Concurrent Start-To-Start dependency on " +
                            $"Prerequisite '{predecessor.TitleSnapshot}' starting on {predecessor.ScheduledStartDate.Value:yyyy-MM-dd}.");
                    }
                }
            }
        }
    }

    // =========================================================================
    // STATE MACHINE TRANSITION METHODS & MATCHMAKING OPERATIONS
    // =========================================================================

    /// <summary>
    /// Issues a new matchmaking request to an academic supervisor. 
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

        if (_supervisionRequests.Any(r => r.Status == SupervisionRequestStatus.Pending))
        {
            throw new InvalidProjectInstanceTransitionException("An active supervision request is already pending review for this project workspace.");
        }

        var request = new SupervisionRequest(Id, professorId, pitchText, utcNow);
        _supervisionRequests.Add(request);

        AddDomainEvent(new SupervisionRequestCreatedEvent(request.Id, Id, professorId, request.PitchText));
    }

    /// <summary>
    /// Processes a professor's evaluation review for an outstanding pending supervision request.
    /// </summary>
    public void ReviewSupervisionRequest(Guid requestId, bool accept, string? rejectionReason, DateTime reviewedAt)
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

        var finalStatus = accept ? SupervisionRequestStatus.Accepted : SupervisionRequestStatus.Rejected;

        request.RecordReview(finalStatus, rejectionReason, reviewedAt);

        if (accept)
        {
            request.Status = SupervisionRequestStatus.Accepted;
            SupervisorId = request.ProfessorId;

            if (Status == ProjectInstanceStatus.AwaitingSupervision)
            {
                Status = ProjectInstanceStatus.Active;
            }

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
            request.RejectionReason = string.IsNullOrWhiteSpace(rejectionReason) ? "Declined" : rejectionReason.Trim();

            AddDomainEvent(new SupervisionRequestRejectedEvent(request.Id, Id, request.ProfessorId, request.RejectionReason));
        }
    }

    /// <summary>
    /// Explicitly transitions an onboarding paused project into an active solo configuration.
    /// </summary>
    public void TransitionToSolo()
    {
        if (Status != ProjectInstanceStatus.AwaitingSupervision)
        {
            throw new InvalidProjectInstanceTransitionException($"Cannot pivot to solo execution. Instance must be 'AwaitingSupervision', but is currently '{Status}'.");
        }

        Status = ProjectInstanceStatus.Active;
        SupervisorId = null;

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
            proposal.RejectionReason = string.IsNullOrWhiteSpace(rejectionReason) ? "Declined" : rejectionReason.Trim();
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
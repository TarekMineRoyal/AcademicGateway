using System;
using System.Linq;
using Xunit;
using AcademicGateway.Domain.Common.Enums;
using AcademicGateway.Domain.ProjectTemplates;
using AcademicGateway.Domain.ProjectTemplates.Enums;
using AcademicGateway.Domain.ProjectTemplates.Exceptions;
using AcademicGateway.Domain.ProjectTemplates.Events;
using AcademicGateway.Domain.ProjectInstances.Services;

namespace AcademicGateway.Domain.UnitTests.ProjectTemplates;

/// <summary>
/// Contains isolated unit tests verifying the structural invariants, domain workflows, 
/// and lifecycle state machine rules of the <see cref="ProjectTemplate"/> aggregate root.
/// </summary>
public class ProjectTemplateTests
{
    private readonly string _validTitle = "Enterprise Cloud Architecture Blueprint";
    private readonly string _validDescription = "Comprehensive guidelines for building multi-region resilient microservices.";
    private readonly Guid _validProviderId = Guid.NewGuid();
    private readonly DateTime _validCreatedAt = DateTime.UtcNow;

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeInDraftStateAndRaiseEvent_WhenParametersAreValid()
    {
        // Arrange & Act
        var template = new ProjectTemplate(_validTitle, _validDescription, _validProviderId, _validCreatedAt);

        // Assert
        Assert.NotEqual(Guid.Empty, template.Id);
        Assert.Equal(_validTitle, template.Title);
        Assert.Equal(_validDescription, template.Description);
        Assert.Equal(_validProviderId, template.ProviderId);
        Assert.Equal(ProjectTemplateStatus.Draft, template.Status);
        Assert.Equal(_validCreatedAt, template.CreatedAt);
        Assert.Null(template.ReviewerFeedback);

        // Verify Domain Event Emission
        Assert.Single(template.DomainEvents);
        var createdEvent = template.DomainEvents.First() as ProjectTemplateCreatedEvent;
        Assert.NotNull(createdEvent);
        Assert.Equal(template.Id, createdEvent.TemplateId);
        Assert.Equal(template.ProviderId, createdEvent.ProviderId);
        Assert.Equal(template.Title, createdEvent.Title);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidTemplateDetailsException_WhenProviderIdIsEmpty()
    {
        // Arrange
        var emptyProviderId = Guid.Empty;

        // Act & Assert
        var exception = Assert.Throws<InvalidTemplateDetailsException>(() =>
            new ProjectTemplate(_validTitle, _validDescription, emptyProviderId, _validCreatedAt));

        Assert.Contains("Provider ID cannot be an empty Guid.", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_ShouldThrowInvalidTemplateDetailsException_WhenTitleIsEmptyOrWhitespace(string? invalidTitle)
    {
        // Act & Assert
        Assert.Throws<InvalidTemplateDetailsException>(() =>
            new ProjectTemplate(invalidTitle!, _validDescription, _validProviderId, _validCreatedAt));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_ShouldThrowInvalidTemplateDetailsException_WhenDescriptionIsEmptyOrWhitespace(string? invalidDescription)
    {
        // Act & Assert
        Assert.Throws<InvalidTemplateDetailsException>(() =>
            new ProjectTemplate(_validTitle, invalidDescription!, _validProviderId, _validCreatedAt));
    }

    #endregion

    #region Milestone Management Tests

    [Fact]
    public void AddMilestone_ShouldAddMilestoneSuccessfully_WhenTemplateIsInDraftState()
    {
        // Arrange
        var template = new ProjectTemplate(_validTitle, _validDescription, _validProviderId, _validCreatedAt);
        var milestoneTitle = "Phase 1: Architecture Sign-off";
        var milestoneDesc = "Submit structural systems schema diagrams.";
        decimal expectedHours = 12.5m;
        var deliverableType = DeliverableType.File;

        // Act
        template.AddMilestone(milestoneTitle, milestoneDesc, expectedHours, deliverableType);

        // Assert
        Assert.Single(template.GlobalMilestones);
        var milestone = template.GlobalMilestones.First();
        Assert.NotEqual(Guid.Empty, milestone.Id);
        Assert.Equal(template.Id, milestone.ProjectTemplateId);
        Assert.Equal(milestoneTitle, milestone.Title);
        Assert.Equal(milestoneDesc, milestone.Description);
        Assert.Equal(expectedHours, milestone.ExpectedEffortInHours);
        Assert.Equal(deliverableType, milestone.RequiredDeliverableType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5.5)]
    public void AddMilestone_ShouldThrowInvalidTemplateDetailsException_WhenExpectedEffortIsZeroOrNegative(decimal invalidHours)
    {
        // Arrange
        var template = new ProjectTemplate(_validTitle, _validDescription, _validProviderId, _validCreatedAt);

        // Act & Assert
        var exception = Assert.Throws<InvalidTemplateDetailsException>(() =>
            template.AddMilestone("Invalid Effort MS", "Description", invalidHours, DeliverableType.Url));

        Assert.Contains("Expected effort must be greater than zero hours.", exception.Message);
    }

    [Fact]
    public void AddMilestone_ShouldThrowInvalidTemplateStatusException_WhenTemplateIsApprovedOrRejected()
    {
        // Arrange
        var approvedTemplate = CreateTemplateInState(ProjectTemplateStatus.Approved);
        var rejectedTemplate = CreateTemplateInState(ProjectTemplateStatus.Rejected);

        // Act & Assert
        Assert.Throws<InvalidTemplateStatusException>(() =>
            approvedTemplate.AddMilestone("MS Title", "MS Desc", 10m, DeliverableType.Url));

        Assert.Throws<InvalidTemplateStatusException>(() =>
            rejectedTemplate.AddMilestone("MS Title", "MS Desc", 10m, DeliverableType.Url));
    }

    #endregion

    #region Milestone Dependency Graph (DAG) Tests

    [Fact]
    public void AddMilestoneDependency_ShouldEstablishDependencySuccessfully_WhenGraphIsAcyclic()
    {
        // Arrange
        var template = new ProjectTemplate(_validTitle, _validDescription, _validProviderId, _validCreatedAt);
        template.AddMilestone("Milestone A", "Desc A", 10m, DeliverableType.Url);
        template.AddMilestone("Milestone B", "Desc B", 15m, DeliverableType.File);

        var milestoneA = template.GlobalMilestones.ElementAt(0);
        var milestoneB = template.GlobalMilestones.ElementAt(1);

        // Act - B depends on A (A must happen first, meaning A is a predecessor of B)
        template.AddMilestoneDependency(milestoneB.Id, milestoneA.Id, DependencyType.FinishToStart);

        // Assert
        Assert.Single(milestoneB.InboundDependencies);
        var dependency = milestoneB.InboundDependencies.First();
        Assert.Equal(milestoneA.Id, dependency.PredecessorId);
        Assert.Equal(milestoneB.Id, dependency.SuccessorId);
        Assert.Equal(DependencyType.FinishToStart, dependency.Type);
    }

    [Fact]
    public void AddMilestoneDependency_ShouldThrowInvalidTemplateDetailsException_WhenEitherMilestoneDoesNotExist()
    {
        // Arrange
        var template = new ProjectTemplate(_validTitle, _validDescription, _validProviderId, _validCreatedAt);
        template.AddMilestone("Milestone A", "Desc A", 10m, DeliverableType.Url);
        var milestoneA = template.GlobalMilestones.First();
        var nonExistentMilestoneId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<InvalidTemplateDetailsException>(() =>
            template.AddMilestoneDependency(milestoneA.Id, nonExistentMilestoneId, DependencyType.FinishToStart));

        Assert.Contains("Both target milestones must exist within this template context.", exception.Message);
    }

    [Fact]
    public void AddMilestoneDependency_ShouldThrowInvalidTemplateStatusException_WhenTemplateIsImmutable()
    {
        // Arrange
        var approvedTemplate = CreateTemplateInState(ProjectTemplateStatus.Approved);
        var placeholderId1 = Guid.NewGuid();
        var placeholderId2 = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<InvalidTemplateStatusException>(() =>
            approvedTemplate.AddMilestoneDependency(placeholderId1, placeholderId2, DependencyType.FinishToStart));
    }

    [Fact]
    public void AddMilestoneDependency_ShouldThrowInvalidOperationException_WhenCircularDependencyIsDetected()
    {
        // Arrange
        var template = new ProjectTemplate(_validTitle, _validDescription, _validProviderId, _validCreatedAt);
        template.AddMilestone("Milestone A", "Desc A", 10m, DeliverableType.Url);
        template.AddMilestone("Milestone B", "Desc B", 15m, DeliverableType.Url);
        template.AddMilestone("Milestone C", "Desc C", 20m, DeliverableType.Url);

        var idA = template.GlobalMilestones.ElementAt(0).Id;
        var idB = template.GlobalMilestones.ElementAt(1).Id;
        var idC = template.GlobalMilestones.ElementAt(2).Id;

        // Build valid chain: A <- B <- C (C depends on B, B depends on A)
        template.AddMilestoneDependency(idB, idA, DependencyType.FinishToStart);
        template.AddMilestoneDependency(idC, idB, DependencyType.FinishToStart);

        // Act & Assert - Attempting to force A to depend on C creates a cyclic loop (A -> B -> C -> A)
        var exception = Assert.Throws<InvalidOperationException>(() =>
            template.AddMilestoneDependency(idA, idC, DependencyType.FinishToStart));

        Assert.Contains("Dependency rejected: Action introduces an invalid circular reference / DAG loop.", exception.Message);
    }

    #endregion

    #region Update Details Tests

    [Fact]
    public void UpdateDetails_ShouldModifyTitleAndDescription_WhenTemplateIsEditable()
    {
        // Arrange
        var template = new ProjectTemplate(_validTitle, _validDescription, _validProviderId, _validCreatedAt);
        var updatedTitle = "  New Cleaned Title  ";
        var updatedDesc = "  New Cleaned Description Guidelines  ";

        // Act
        template.UpdateDetails(updatedTitle, updatedDesc);

        // Assert
        Assert.Equal("New Cleaned Title", template.Title);
        Assert.Equal("New Cleaned Description Guidelines", template.Description);
    }

    [Fact]
    public void UpdateDetails_ShouldThrowInvalidTemplateStatusException_WhenTemplateIsLocked()
    {
        // Arrange
        var approvedTemplate = CreateTemplateInState(ProjectTemplateStatus.Approved);

        // Act & Assert
        Assert.Throws<InvalidTemplateStatusException>(() =>
            approvedTemplate.UpdateDetails("New Title", "New Description"));
    }

    #endregion

    #region Workflow Pipeline State Machine Tests

    [Theory]
    [InlineData(ProjectTemplateStatus.Draft)]
    [InlineData(ProjectTemplateStatus.ChangesRequested)]
    public void SubmitForReview_ShouldTransitionToPendingReviewAndRaiseEvent_WhenStateIsValid(ProjectTemplateStatus initialStatus)
    {
        // Arrange
        var template = CreateTemplateInState(initialStatus);
        template.ClearDomainEvents();

        // Act
        template.SubmitForReview();

        // Assert
        Assert.Equal(ProjectTemplateStatus.PendingReview, template.Status);
        Assert.Single(template.DomainEvents);
        var subEvent = template.DomainEvents.First() as ProjectTemplateSubmittedEvent;
        Assert.NotNull(subEvent);
        Assert.Equal(template.Id, subEvent.TemplateId);
    }

    [Fact]
    public void SubmitForReview_ShouldThrowInvalidTemplateStatusException_WhenAlreadyPendingOrApproved()
    {
        // Arrange
        var pendingTemplate = CreateTemplateInState(ProjectTemplateStatus.PendingReview);

        // Act & Assert
        Assert.Throws<InvalidTemplateStatusException>(() => pendingTemplate.SubmitForReview());
    }

    [Fact]
    public void Approve_ShouldTransitionToApprovedAndClearFeedback_WhenInPendingReviewState()
    {
        // Arrange
        var template = CreateTemplateInState(ProjectTemplateStatus.PendingReview);
        template.ClearDomainEvents();

        // Act
        template.Approve();

        // Assert
        Assert.Equal(ProjectTemplateStatus.Approved, template.Status);
        Assert.Null(template.ReviewerFeedback);

        Assert.Single(template.DomainEvents);
        Assert.IsType<ProjectTemplateApprovedEvent>(template.DomainEvents.First());
    }

    [Fact]
    public void RequestChanges_ShouldTransitionToChangesRequestedAndSetFeedback_WhenInPendingReviewState()
    {
        // Arrange
        var template = CreateTemplateInState(ProjectTemplateStatus.PendingReview);
        template.ClearDomainEvents();
        var feedbackMessage = "  Please add more descriptive metrics to Phase 2.  ";

        // Act
        template.RequestChanges(feedbackMessage);

        // Assert
        Assert.Equal(ProjectTemplateStatus.ChangesRequested, template.Status);
        Assert.Equal("Please add more descriptive metrics to Phase 2.", template.ReviewerFeedback);

        Assert.Single(template.DomainEvents);
        var feedbackEvent = template.DomainEvents.First() as ProjectTemplateChangesRequestedEvent;
        Assert.NotNull(feedbackEvent);
        Assert.Equal("Please add more descriptive metrics to Phase 2.", feedbackEvent.Feedback);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void RequestChanges_ShouldThrowInvalidTemplateDetailsException_WhenFeedbackIsEmpty(string? invalidFeedback)
    {
        // Arrange
        var template = CreateTemplateInState(ProjectTemplateStatus.PendingReview);

        // Act & Assert
        Assert.Throws<InvalidTemplateDetailsException>(() => template.RequestChanges(invalidFeedback!));
    }

    [Fact]
    public void ProposeReviewerChanges_ShouldUpdateDetailsAndSetPendingProviderAcceptance_WhenInPendingReviewState()
    {
        // Arrange
        var template = CreateTemplateInState(ProjectTemplateStatus.PendingReview);
        template.ClearDomainEvents();
        var adjustedTitle = "Adjusted Core Track Architecture";
        var adjustedDesc = "Adjusted Description Matrix.";

        // Act
        template.ProposeReviewerChanges(adjustedTitle, adjustedDesc);

        // Assert
        Assert.Equal(ProjectTemplateStatus.PendingProviderAcceptance, template.Status);
        Assert.Equal(adjustedTitle, template.Title);
        Assert.Equal(adjustedDesc, template.Description);
        Assert.Equal("Reviewer has modified details. Awaiting provider confirmation.", template.ReviewerFeedback);

        Assert.Single(template.DomainEvents);
        Assert.IsType<ProjectTemplateReviewerChangesProposedEvent>(template.DomainEvents.First());
    }

    [Fact]
    public void ProviderAcceptProposedChanges_ShouldTransitionToApproved_WhenInPendingProviderAcceptanceState()
    {
        // Arrange
        var template = CreateTemplateInState(ProjectTemplateStatus.PendingProviderAcceptance);
        template.ClearDomainEvents();

        // Act
        template.ProviderAcceptProposedChanges();

        // Assert
        Assert.Equal(ProjectTemplateStatus.Approved, template.Status);
        Assert.Null(template.ReviewerFeedback);

        Assert.Single(template.DomainEvents);
        Assert.IsType<ProjectTemplateApprovedEvent>(template.DomainEvents.First());
    }

    [Fact]
    public void ProviderRejectProposedChanges_ShouldTransitionToDraft_WhenInPendingProviderAcceptanceState()
    {
        // Arrange
        var template = CreateTemplateInState(ProjectTemplateStatus.PendingProviderAcceptance);
        template.ClearDomainEvents();

        // Act
        template.ProviderRejectProposedChanges();

        // Assert
        Assert.Equal(ProjectTemplateStatus.Draft, template.Status);
        Assert.Equal("Provider declined reviewer alterations. Reverted back to draft layout.", template.ReviewerFeedback);

        Assert.Single(template.DomainEvents);
        Assert.IsType<ProjectTemplateReviewerChangesRejectedEvent>(template.DomainEvents.First());
    }

    [Fact]
    public void RejectPermanently_ShouldTransitionToRejectedAndSetFeedback_WhenInPendingReviewState()
    {
        // Arrange
        var template = CreateTemplateInState(ProjectTemplateStatus.PendingReview);
        template.ClearDomainEvents();
        var rejectionReason = "Violates core corporate platform policies.";

        // Act
        template.RejectPermanently(rejectionReason);

        // Assert
        Assert.Equal(ProjectTemplateStatus.Rejected, template.Status);
        Assert.Equal(rejectionReason, template.ReviewerFeedback);

        Assert.Single(template.DomainEvents);
        var rejectEvent = template.DomainEvents.First() as ProjectTemplateRejectedPermanentlyEvent;
        Assert.NotNull(rejectEvent);
        Assert.Equal(rejectionReason, rejectEvent.Reason);
    }

    #endregion

    #region Skill Matrix Mapping Tests

    [Fact]
    public void AddSkill_ShouldAddSkillSuccessfully_WhenSkillIsValidAndUnderLimit()
    {
        // Arrange
        var template = new ProjectTemplate(_validTitle, _validDescription, _validProviderId, _validCreatedAt);
        var skillId = Guid.NewGuid();

        // Act
        template.AddSkill(skillId);

        // Assert
        Assert.Single(template.ProjectTemplateSkills);
        Assert.Equal(skillId, template.ProjectTemplateSkills.First().SkillId);
    }

    [Fact]
    public void AddSkill_ShouldThrowInvalidTemplateDetailsException_WhenSkillIdIsEmpty()
    {
        // Arrange
        var template = new ProjectTemplate(_validTitle, _validDescription, _validProviderId, _validCreatedAt);

        // Act & Assert
        Assert.Throws<InvalidTemplateDetailsException>(() => template.AddSkill(Guid.Empty));
    }

    [Fact]
    public void AddSkill_ShouldIgnoreDuplicateSkill_WhenSkillIsAlreadyAdded()
    {
        // Arrange
        var template = new ProjectTemplate(_validTitle, _validDescription, _validProviderId, _validCreatedAt);
        var skillId = Guid.NewGuid();
        template.AddSkill(skillId);

        // Act
        template.AddSkill(skillId);

        // Assert
        Assert.Single(template.ProjectTemplateSkills);
    }

    [Fact]
    public void AddSkill_ShouldThrowInvalidTemplateDetailsException_WhenSkillLimitIsExceeded()
    {
        // Arrange
        var template = new ProjectTemplate(_validTitle, _validDescription, _validProviderId, _validCreatedAt);
        for (int i = 0; i < 10; i++)
        {
            template.AddSkill(Guid.NewGuid());
        }

        // Act & Assert
        var exception = Assert.Throws<InvalidTemplateDetailsException>(() => template.AddSkill(Guid.NewGuid()));
        Assert.Contains("A single project template cannot require more than 10 technical skills.", exception.Message);
    }

    [Fact]
    public void RemoveSkill_ShouldRemoveSkillSuccessfully_WhenSkillExists()
    {
        // Arrange
        var template = new ProjectTemplate(_validTitle, _validDescription, _validProviderId, _validCreatedAt);
        var skillId = Guid.NewGuid();
        template.AddSkill(skillId);

        // Act
        template.RemoveSkill(skillId);

        // Assert
        Assert.Empty(template.ProjectTemplateSkills);
    }

    #endregion

    #region Instantiation Blueprint Factory Tests

    [Fact]
    public void Instantiate_ShouldThrowInvalidTemplateStatusException_WhenTemplateIsNotApproved()
    {
        // Arrange
        var draftTemplate = CreateTemplateInState(ProjectTemplateStatus.Draft);
        var factory = new LocalMilestoneFactory();

        // Act & Assert
        Assert.Throws<InvalidTemplateStatusException>(() =>
            draftTemplate.Instantiate(Guid.NewGuid(), DateTime.UtcNow, factory));
    }

    [Fact]
    public void Instantiate_ShouldThrowInvalidTemplateDetailsException_WhenStudentIdIsEmpty()
    {
        // Arrange
        var approvedTemplate = CreateTemplateInState(ProjectTemplateStatus.Approved);
        var factory = new LocalMilestoneFactory();

        // Act & Assert
        Assert.Throws<InvalidTemplateDetailsException>(() =>
            approvedTemplate.Instantiate(Guid.Empty, DateTime.UtcNow, factory));
    }

    [Fact]
    public void Instantiate_ShouldThrowArgumentNullException_WhenMilestoneFactoryIsNull()
    {
        // Arrange
        var approvedTemplate = CreateTemplateInState(ProjectTemplateStatus.Approved);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            approvedTemplate.Instantiate(Guid.NewGuid(), DateTime.UtcNow, null!));
    }

    #endregion

    #region State Machine Test Generation Helper

    /// <summary>
    /// Helper factory executing structural state transition combinations onto a 
    /// ProjectTemplate aggregate block natively to set up testing contexts cleanly.
    /// </summary>
    private ProjectTemplate CreateTemplateInState(ProjectTemplateStatus targetedStatus)
    {
        var template = new ProjectTemplate(_validTitle, _validDescription, _validProviderId, _validCreatedAt);

        if (targetedStatus == ProjectTemplateStatus.Draft)
            return template;

        // Move to PendingReview
        template.SubmitForReview();
        if (targetedStatus == ProjectTemplateStatus.PendingReview)
            return template;

        switch (targetedStatus)
        {
            case ProjectTemplateStatus.Approved:
                template.Approve();
                break;
            case ProjectTemplateStatus.ChangesRequested:
                template.RequestChanges("Need structural optimization feedback changes.");
                break;
            case ProjectTemplateStatus.PendingProviderAcceptance:
                template.ProposeReviewerChanges("Adjusted Title Track", "Adjusted Description");
                break;
            case ProjectTemplateStatus.Rejected:
                template.RejectPermanently("Permanent Policy Violation.");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(targetedStatus), "State creation track helper not supported.");
        }

        return template;
    }

    #endregion
}
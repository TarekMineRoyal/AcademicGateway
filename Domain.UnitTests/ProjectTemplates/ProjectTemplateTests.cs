using System;
using System.Linq;
using AcademicGateway.Domain.Common.Enums;
using AcademicGateway.Domain.ProjectInstances.Services;
using AcademicGateway.Domain.ProjectTemplates;
using AcademicGateway.Domain.ProjectTemplates.Enums;
using AcademicGateway.Domain.ProjectTemplates.Events;
using AcademicGateway.Domain.ProjectTemplates.Exceptions;
using AcademicGateway.Domain.ProjectInstances.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace AcademicGateway.Domain.UnitTests.ProjectTemplates;

public class ProjectTemplateTests
{
    private readonly Guid _fallbackProviderId = Guid.NewGuid();
    private readonly string _fallbackTitle = "E-Commerce Cloud Architecture";
    private readonly string _fallbackDesc = "Design and deploy an enterprise microservices e-commerce platform.";
    private readonly DateTime _fallbackTime = new(2026, 7, 8, 12, 0, 0, DateTimeKind.Utc);

    private ProjectTemplate CreateDraftTemplate()
    {
        return new ProjectTemplate(_fallbackTitle, _fallbackDesc, _fallbackProviderId, _fallbackTime);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeInDraftState_WhenParametersAreValid()
    {
        // Arrange
        var untrimmedTitle = "   " + _fallbackTitle + "   ";
        var untrimmedDesc = "   " + _fallbackDesc + "   ";

        // Act
        var template = new ProjectTemplate(untrimmedTitle, untrimmedDesc, _fallbackProviderId, _fallbackTime);

        // Assert
        template.Id.Should().NotBeEmpty();
        template.ProviderId.Should().Be(_fallbackProviderId);
        template.Title.Should().Be(_fallbackTitle);
        template.Description.Should().Be(_fallbackDesc);
        template.Status.Should().Be(ProjectTemplateStatus.Draft);
        template.CreatedAt.Should().Be(_fallbackTime);
        template.ReviewerFeedback.Should().BeNull();
        template.GlobalMilestones.Should().BeEmpty();
        template.ProjectTemplateSkills.Should().BeEmpty();

        template.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProjectTemplateCreatedEvent>()
            .Which.Should().Match<ProjectTemplateCreatedEvent>(e =>
                e.TemplateId == template.Id && e.ProviderId == _fallbackProviderId && e.Title == _fallbackTitle);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidTemplateDetailsException_WhenProviderIdIsEmpty()
    {
        // Act
        Action act = () => _ = new ProjectTemplate("Title", "Desc", Guid.Empty, _fallbackTime);

        // Assert
        act.Should().Throw<InvalidTemplateDetailsException>()
           .WithMessage("Provider ID cannot be an empty Guid.");
    }

    #endregion

    #region UpdateDetails Tests

    [Fact]
    public void UpdateDetails_ShouldModifyTextAndTrim_WhenStatusIsEditable()
    {
        // Arrange
        var template = CreateDraftTemplate();

        // Act
        template.UpdateDetails("  New Title  ", "  New Description Specification  ");

        // Assert
        template.Title.Should().Be("New Title");
        template.Description.Should().Be("New Description Specification");
    }

    [Theory]
    [InlineData(null, "Valid Description")]
    [InlineData("", "Valid Description")]
    [InlineData("   ", "Valid Description")]
    [InlineData("Valid Title", null)]
    [InlineData("Valid Title", "")]
    [InlineData("Valid Title", "   ")]
    public void UpdateDetails_ShouldThrowInvalidTemplateDetailsException_WhenInputsAreInvalid(string? title, string? desc)
    {
        // Arrange
        var template = CreateDraftTemplate();

        // Act
        Action act = () => template.UpdateDetails(title!, desc!);

        // Assert
        act.Should().Throw<InvalidTemplateDetailsException>();
    }

    #endregion

    #region Milestone & Dependency DAG Tests

    [Fact]
    public void AddMilestone_ShouldAppendToCollection_WhenStatusIsEditableAndEffortIsPositive()
    {
        // Arrange
        var template = CreateDraftTemplate();

        // Act
        template.AddMilestone("Requirement Gathering", "Document all features", 15.5m, DeliverableType.File);

        // Assert
        template.GlobalMilestones.Should().ContainSingle()
            .Which.Should().Match<GlobalMilestone>(m =>
                m.Title == "Requirement Gathering" &&
                m.Description == "Document all features" &&
                m.ExpectedEffortInHours == 15.5m &&
                m.RequiredDeliverableType == DeliverableType.File);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void AddMilestone_ShouldThrowInvalidTemplateDetailsException_WhenEffortIsZeroOrNegative(decimal invalidEffort)
    {
        // Arrange
        var template = CreateDraftTemplate();

        // Act
        Action act = () => template.AddMilestone("Title", "Desc", invalidEffort, DeliverableType.Url);

        // Assert
        act.Should().Throw<InvalidTemplateDetailsException>()
           .WithMessage("Expected effort must be greater than zero hours.");
    }

    [Fact]
    public void AddMilestoneDependency_ShouldEstablishLink_WhenNoCycleIsIntroduced()
    {
        // Arrange
        var template = CreateDraftTemplate();
        template.AddMilestone("Milestone A", "Desc", 10, DeliverableType.File);
        template.AddMilestone("Milestone B", "Desc", 20, DeliverableType.Url);

        var mA = template.GlobalMilestones.ElementAt(0);
        var mB = template.GlobalMilestones.ElementAt(1);

        // Act
        template.AddMilestoneDependency(mB.Id, mA.Id, DependencyType.FinishToStart);

        // Assert
        mB.InboundDependencies.Should().ContainSingle()
            .Which.PredecessorId.Should().Be(mA.Id);
    }

    [Fact]
    public void AddMilestoneDependency_ShouldThrowCircularDependencyException_WhenLoopIsDetected()
    {
        // Arrange
        var template = CreateDraftTemplate();
        template.AddMilestone("Milestone A", "Desc", 10, DeliverableType.File);
        template.AddMilestone("Milestone B", "Desc", 15, DeliverableType.File);
        template.AddMilestone("Milestone C", "Desc", 20, DeliverableType.File);

        var idA = template.GlobalMilestones.ElementAt(0).Id;
        var idB = template.GlobalMilestones.ElementAt(1).Id;
        var idC = template.GlobalMilestones.ElementAt(2).Id;

        template.AddMilestoneDependency(idB, idA, DependencyType.FinishToStart);
        template.AddMilestoneDependency(idC, idB, DependencyType.FinishToStart);

        // Act & Assert
        Action act = () => template.AddMilestoneDependency(idA, idC, DependencyType.FinishToStart);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Dependency rejected: Action introduces an invalid circular reference / DAG loop.");
    }

    #endregion

    #region Pipeline State Machine Flow Tests

    [Fact]
    public void ReviewPipeline_CompleteSuccessfulFlow_ShouldTransitionStatesCorrectly()
    {
        var template = CreateDraftTemplate();
        template.Status.Should().Be(ProjectTemplateStatus.Draft);

        template.SubmitForReview();
        template.Status.Should().Be(ProjectTemplateStatus.PendingReview);
        template.DomainEvents.Should().Contain(e => e is ProjectTemplateSubmittedEvent);

        template.RequestChanges(" Fix formatting rule 3. ");
        template.Status.Should().Be(ProjectTemplateStatus.ChangesRequested);
        template.ReviewerFeedback.Should().Be("Fix formatting rule 3.");
        template.DomainEvents.Should().Contain(e => e is ProjectTemplateChangesRequestedEvent);

        template.SubmitForReview();
        template.Status.Should().Be(ProjectTemplateStatus.PendingReview);

        template.Approve();
        template.Status.Should().Be(ProjectTemplateStatus.Approved);
        template.ReviewerFeedback.Should().BeNull();
        template.DomainEvents.Should().Contain(e => e is ProjectTemplateApprovedEvent);
    }

    [Fact]
    public void RequestChanges_ShouldThrowInvalidTemplateDetailsException_WhenFeedbackIsBlank()
    {
        // Arrange
        var template = CreateDraftTemplate();
        template.SubmitForReview();

        // Act
        Action act = () => template.RequestChanges("   ");

        // Assert
        act.Should().Throw<InvalidTemplateDetailsException>()
           .WithMessage("Feedback instructions must be provided to guide the provider's corrections.");
    }

    [Fact]
    public void ProposeReviewerChanges_Workflow_ShouldDirectlySettleViaProviderAcceptance()
    {
        // Arrange
        var template = CreateDraftTemplate();
        template.SubmitForReview();

        // Act
        template.ProposeReviewerChanges("Refined Title", "Refined Description Structural Parameters");

        // Assert
        template.Status.Should().Be(ProjectTemplateStatus.PendingProviderAcceptance);
        template.Title.Should().Be("Refined Title");
        template.Description.Should().Be("Refined Description Structural Parameters");
        template.ReviewerFeedback.Should().Be("Reviewer has modified details. Awaiting provider confirmation.");
        template.DomainEvents.Should().Contain(e => e is ProjectTemplateReviewerChangesProposedEvent);

        // Act
        template.ProviderAcceptProposedChanges();

        // Assert
        template.Status.Should().Be(ProjectTemplateStatus.Approved);
        template.ReviewerFeedback.Should().BeNull();
    }

    [Fact]
    public void ProposeReviewerChanges_Workflow_ShouldRevertToDraft_WhenProviderDeclines()
    {
        // Arrange
        var template = CreateDraftTemplate();
        template.SubmitForReview();
        template.ProposeReviewerChanges("Refined Title", "Refined Description");

        // Act
        template.ProviderRejectProposedChanges();

        // Assert
        template.Status.Should().Be(ProjectTemplateStatus.Draft);
        template.ReviewerFeedback.Should().Be("Provider declined reviewer alterations. Reverted back to draft layout.");
        template.DomainEvents.Should().Contain(e => e is ProjectTemplateReviewerChangesRejectedEvent);
    }

    [Fact]
    public void RejectPermanently_ShouldLockTemplate_WhenStatusIsPendingReview()
    {
        // Arrange
        var template = CreateDraftTemplate();
        template.SubmitForReview();

        // Act
        template.RejectPermanently("  Out of scope for this academic cycle.  ");

        // Assert
        template.Status.Should().Be(ProjectTemplateStatus.Rejected);
        template.ReviewerFeedback.Should().Be("Out of scope for this academic cycle.");
        template.DomainEvents.Should().Contain(e => e is ProjectTemplateRejectedPermanentlyEvent);
    }

    [Theory]
    [InlineData(ProjectTemplateStatus.Approved)]
    [InlineData(ProjectTemplateStatus.Rejected)]
    public void Operations_ShouldThrowInvalidTemplateStatusException_WhenTemplateIsLocked(ProjectTemplateStatus lockedStatus)
    {
        // Arrange
        var template = CreateDraftTemplate();
        template.SubmitForReview();

        if (lockedStatus == ProjectTemplateStatus.Approved)
            template.Approve();
        else
            template.RejectPermanently("Refused");

        // Act
        Action actUpdate = () => template.UpdateDetails("Title", "Desc");
        Action actAddMilestone = () => template.AddMilestone("T", "D", 5, DeliverableType.Url);

        // Assert
        actUpdate.Should().Throw<InvalidTemplateStatusException>();
        actAddMilestone.Should().Throw<InvalidTemplateStatusException>();
    }

    #endregion

    #region Skill Matrix Capacity Tests

    [Fact]
    public void AddSkill_ShouldAppendSkill_WhenIdIsValidAndUnique()
    {
        // Arrange
        var template = CreateDraftTemplate();
        var skillId = Guid.NewGuid();

        // Act
        template.AddSkill(skillId);

        // Assert
        template.ProjectTemplateSkills.Should().ContainSingle(s => s.SkillId == skillId);
    }

    [Fact]
    public void AddSkill_ShouldIgnoreDuplicate_WhenSkillAlreadyAdded()
    {
        // Arrange
        var template = CreateDraftTemplate();
        var skillId = Guid.NewGuid();
        template.AddSkill(skillId);

        // Act
        template.AddSkill(skillId);

        // Assert
        template.ProjectTemplateSkills.Should().HaveCount(1);
    }

    [Fact]
    public void AddSkill_ShouldThrowInvalidTemplateDetailsException_WhenSkillsExceedLimitOf10()
    {
        // Arrange
        var template = CreateDraftTemplate();
        for (int i = 0; i < 10; i++)
        {
            template.AddSkill(Guid.NewGuid());
        }

        // Act
        Action act = () => template.AddSkill(Guid.NewGuid());

        // Assert
        act.Should().Throw<InvalidTemplateDetailsException>()
           .WithMessage("A single project template cannot require more than 10 technical skills.");
    }

    [Fact]
    public void RemoveSkill_ShouldEvictSkill_WhenSkillExists()
    {
        // Arrange
        var template = CreateDraftTemplate();
        var skillId = Guid.NewGuid();
        template.AddSkill(skillId);

        // Act
        template.RemoveSkill(skillId);

        // Assert
        template.ProjectTemplateSkills.Should().BeEmpty();
    }

    #endregion

    #region Prototype Instantiation Pattern Tests

    [Fact]
    public void Instantiate_ShouldProduceFullyPopulatedProjectInstance_WhenTemplateIsApproved()
    {
        // Arrange
        var template = CreateDraftTemplate();
        template.SubmitForReview();
        template.Approve();

        var studentId = Guid.NewGuid();
        var executionTime = DateTime.UtcNow;

        var factoryMock = new Mock<LocalMilestoneFactory>();

        // Act
        var instance = template.Instantiate(studentId, executionTime, factoryMock.Object);

        // Assert
        instance.Should().NotBeNull();
        instance.TemplateId.Should().Be(template.Id);
        instance.StudentId.Should().Be(studentId);
        instance.ProviderId.Should().Be(template.ProviderId);
        instance.TitleSnapshot.Should().Be(template.Title);
        instance.DescriptionSnapshot.Should().Be(template.Description);
        instance.Status.Should().Be(ProjectInstanceStatus.Active);
    }

    [Fact]
    public void Instantiate_ShouldThrowInvalidTemplateStatusException_WhenTemplateIsNotApproved()
    {
        // Arrange
        var unapprovedTemplate = CreateDraftTemplate();
        var factoryMock = new Mock<LocalMilestoneFactory>();

        // Act
        Action act = () => unapprovedTemplate.Instantiate(Guid.NewGuid(), DateTime.UtcNow, factoryMock.Object);

        // Assert
        act.Should().Throw<InvalidTemplateStatusException>();
    }

    #endregion
}
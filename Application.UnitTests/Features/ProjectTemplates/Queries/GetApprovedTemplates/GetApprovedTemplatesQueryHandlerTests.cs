using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProjectTemplates.Queries.GetApprovedTemplates;
using AcademicGateway.Domain.ProjectTemplates;
using AcademicGateway.Domain.ProjectTemplates.Enums;
using AcademicGateway.Domain.Providers;
using AcademicGateway.Domain.Skills;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.ProjectTemplates.Queries.GetApprovedTemplates;

/// <summary>
/// Contains isolated unit verification routines for the <see cref="GetApprovedTemplatesQueryHandler"/>.
/// Validates relational read-only projections, conditional database filtering criteria, 
/// workflow status isolation barriers, and projection ternary null-coalescing string fallbacks.
/// </summary>
public class GetApprovedTemplatesQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly GetApprovedTemplatesQueryHandler _handler;

    /// <summary>
    /// Initializes a pristine instance of the test class with isolated database mock mappings.
    /// </summary>
    public GetApprovedTemplatesQueryHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new GetApprovedTemplatesQueryHandler(_mockContext.Object);
    }

    /// <summary>
    /// Assures that executing an unfiltered query retrieves all project templates that have successfully 
    /// advanced into an Approved pipeline state while entirely ignoring other operational lifecycles.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNoFiltersApplied_ShouldReturnAllApprovedTemplatesDirectly()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var queryPayload = new GetApprovedTemplatesQuery { SkillId = null };

        var provider = new Provider(providerId, "Cloud Solutions Corp");

        // Precondition 1: Setup Approved Listing via standard aggregate state machine transitions
        var templateApproved = new ProjectTemplate(
            title: "Cloud Ops Masterclass",
            description: "Description lengthy enough to bypass validation parameters within the system layout.",
            providerId: providerId);
        templateApproved.SubmitForReview();
        templateApproved.Approve();
        SetPrivateProperty(templateApproved, nameof(ProjectTemplate.Provider), provider);

        // Precondition 2: Setup non-approved listings that should be automatically skipped by the query block
        var templateDraft = new ProjectTemplate("Draft Track Outline", "Valid description placeholder metrics.", providerId);

        var templatePending = new ProjectTemplate("Pending Review Track", "Valid description placeholder metrics.", providerId);
        templatePending.SubmitForReview();

        var templateRejected = new ProjectTemplate("Rejected Track Outline", "Valid description placeholder metrics.", providerId);
        templateRejected.SubmitForReview();
        templateRejected.RejectPermanently("Fails core curriculum criteria framework bounds.");

        var templatePool = new List<ProjectTemplate> { templateApproved, templateDraft, templatePending, templateRejected };
        _mockContext.Setup(c => c.ProjectTemplates).Returns(templatePool.BuildMockDbSet().Object);

        // Act
        var resultList = await _handler.Handle(queryPayload, TestContext.Current.CancellationToken);

        // Assert
        resultList.Should().HaveCount(1);

        var projectedItem = resultList.First();
        projectedItem.Title.Should().Be("Cloud Ops Masterclass");
        projectedItem.ProviderCompanyName.Should().Be("Cloud Solutions Corp");
    }

    /// <summary>
    /// Assures that supplying a specific Skill unique identifier criterion conditionally alters the lookup base, 
    /// returning only project blueprints that capture that technical competency requirements flag.
    /// </summary>
    [Fact]
    public async Task Handle_GivenSkillFilterSupplied_ShouldReturnOnlyTemplatesMatchingTargetSkillId()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var targetSearchSkillId = Guid.NewGuid();
        var alternativeSkillId = Guid.NewGuid();
        var queryPayload = new GetApprovedTemplatesQuery { SkillId = targetSearchSkillId };

        var provider = new Provider(providerId, "React Development Hub");

        // Template A: Contains the target searching skill competency link
        var templateMatching = new ProjectTemplate(
            title: "Target Tech Track",
            description: "Description lengthy enough to bypass validation parameters within the system layout.",
            providerId: providerId);
        templateMatching.SubmitForReview();
        templateMatching.Approve();
        templateMatching.AddSkill(targetSearchSkillId);
        SetPrivateProperty(templateMatching, nameof(ProjectTemplate.Provider), provider);

        // Simulate relational navigation database hydration for the projection mapping check
        var targetSkillObj = CreateEntityWithPrivateConstructor<Skill>();
        SetPrivateProperty(targetSkillObj, nameof(Skill.Name), "React.js Framework");
        SetPrivateProperty(templateMatching.ProjectTemplateSkills.First(), nameof(ProjectTemplateSkill.Skill), targetSkillObj);

        // Template B: Contains a completely separate skill competency link (Should be skipped)
        var templateMismatched = new ProjectTemplate(
            title: "Alternative Tech Track",
            description: "Description lengthy enough to bypass validation parameters within the system layout.",
            providerId: providerId);
        templateMismatched.SubmitForReview();
        templateMismatched.Approve();
        templateMismatched.AddSkill(alternativeSkillId);
        SetPrivateProperty(templateMismatched, nameof(ProjectTemplate.Provider), provider);

        var templatePool = new List<ProjectTemplate> { templateMatching, templateMismatched };
        _mockContext.Setup(c => c.ProjectTemplates).Returns(templatePool.BuildMockDbSet().Object);

        // Act
        var resultList = await _handler.Handle(queryPayload, TestContext.Current.CancellationToken);

        // Assert
        resultList.Should().HaveCount(1);

        var matchResult = resultList.First();
        matchResult.Title.Should().Be("Target Tech Track");
        matchResult.Skills.Should().HaveCount(1);
        matchResult.Skills.First().Id.Should().Be(targetSearchSkillId);
        matchResult.Skills.First().Name.Should().Be("React.js Framework");
    }

    /// <summary>
    /// Assures that if a project template record does not possess a loaded or referenced Provider entity model,
    /// the dynamic projection safely applies the custom string ternary fallback option "Unknown Provider".
    /// </summary>
    [Fact]
    public async Task Handle_GivenTemplateWithNullProviderRelation_ShouldMapProviderCompanyNameAsUnknownProvider()
    {
        // Arrange
        var queryPayload = new GetApprovedTemplatesQuery { SkillId = null };

        var templateWithoutProvider = new ProjectTemplate(
            title: "Orphaned Platform Template",
            description: "Description lengthy enough to bypass validation parameters within the system layout.",
            providerId: Guid.NewGuid());
        templateWithoutProvider.SubmitForReview();
        templateWithoutProvider.Approve();

        // Explicit Boundary Condition: Leave the 'Provider' navigation property as null
        SetPrivateProperty(templateWithoutProvider, nameof(ProjectTemplate.Provider), null!);

        var templatePool = new List<ProjectTemplate> { templateWithoutProvider };
        _mockContext.Setup(c => c.ProjectTemplates).Returns(templatePool.BuildMockDbSet().Object);

        // Act
        var resultList = await _handler.Handle(queryPayload, TestContext.Current.CancellationToken);

        // Assert
        resultList.Should().HaveCount(1);
        resultList.First().ProviderCompanyName.Should().Be("Unknown Provider");
    }

    /// <summary>
    /// Assures that if a template's skill join reference does not carry a hydrated core Skill domain entity snapshot,
    /// the projection statement gracefully defaults its string descriptor assignment rule to "Unknown Skill".
    /// </summary>
    [Fact]
    public async Task Handle_GivenTemplateWithNullSkillNavigationProperty_ShouldMapSkillNameAsUnknownSkill()
    {
        // Arrange
        var targetSkillId = Guid.NewGuid();
        var queryPayload = new GetApprovedTemplatesQuery { SkillId = null };

        var templateWithMissingSkillNav = new ProjectTemplate(
            title: "Unmapped Competency Architecture",
            description: "Description lengthy enough to bypass validation parameters within the system layout.",
            providerId: Guid.NewGuid());
        templateWithMissingSkillNav.SubmitForReview();
        templateWithMissingSkillNav.Approve();
        templateWithMissingSkillNav.AddSkill(targetSkillId);

        // Explicit Boundary Condition: Force the sub-collection's nested 'Skill' lookups to remain null
        var joinRelation = templateWithMissingSkillNav.ProjectTemplateSkills.First();
        SetPrivateProperty(joinRelation, nameof(ProjectTemplateSkill.Skill), null!);

        var templatePool = new List<ProjectTemplate> { templateWithMissingSkillNav };
        _mockContext.Setup(c => c.ProjectTemplates).Returns(templatePool.BuildMockDbSet().Object);

        // Act
        var resultList = await _handler.Handle(queryPayload, TestContext.Current.CancellationToken);

        // Assert
        resultList.Should().HaveCount(1);
        var mappedSkills = resultList.First().Skills;
        mappedSkills.Should().HaveCount(1);
        mappedSkills.First().Id.Should().Be(targetSkillId);
        mappedSkills.First().Name.Should().Be("Unknown Skill");
    }

    private static T CreateEntityWithPrivateConstructor<T>() =>
        (T)Activator.CreateInstance(typeof(T), true)!;

    private static void SetPrivateProperty(object obj, string propertyName, object? value) =>
        obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?
           .SetValue(obj, value);
}
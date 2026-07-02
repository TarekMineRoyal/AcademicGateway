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
/// Validates relational read-only read projections, conditional criteria matching, and workflow status filters.
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
    /// advanced into an Approved pipeline state while entirely ignoring Draft layouts.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNoFiltersApplied_ShouldReturnAllApprovedTemplatesDirectly()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var queryPayload = new GetApprovedTemplatesQuery { SkillId = null };

        // Best Practice: Populate corporate context to avoid falling back to "Unknown Provider" during projection mapping
        var provider = new Provider(providerId, "Cloud Solutions Corp");

        // Setup Approved Listing via standard aggregate state machine transitions
        var templateApproved = new ProjectTemplate(
            title: "Cloud Ops Masterclass",
            description: "Description lengthy enough to bypass validation parameters within the system layout.",
            providerId: providerId);

        templateApproved.SubmitForReview();
        templateApproved.Approve();
        SetPrivateProperty(templateApproved, nameof(ProjectTemplate.Provider), provider);

        // Setup Draft Listing (Should be filtered out naturally by the query engine)
        var templateDraft = new ProjectTemplate(
            title: "Secret Pending Core Draft",
            description: "Description lengthy enough to bypass validation parameters within the system layout.",
            providerId: providerId);

        var templatePool = new List<ProjectTemplate> { templateApproved, templateDraft };
        _mockContext.Setup(c => c.ProjectTemplates).Returns(templatePool.BuildMockDbSet().Object);

        // Act
        // Best Practice (xUnit1051): Pass TestContext.Current.CancellationToken for responsive test execution controls.
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

    private static T CreateEntityWithPrivateConstructor<T>() =>
        (T)Activator.CreateInstance(typeof(T), true)!;

    private static void SetPrivateProperty(object obj, string propertyName, object value) =>
        obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?
           .SetValue(obj, value);
}
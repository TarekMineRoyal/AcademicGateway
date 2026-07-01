using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProjectTemplates.Queries.GetApprovedTemplates;
using Domain.Lookups;
using Domain.ProjectTemplates;
using Domain.ProjectTemplates.Enums;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.ProjectTemplates.Queries.GetApprovedTemplates;

public class GetApprovedTemplatesQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly GetApprovedTemplatesQueryHandler _handler;

    public GetApprovedTemplatesQueryHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new GetApprovedTemplatesQueryHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_NoFiltersApplied_ShouldReturnAllApprovedTemplatesDirectly()
    {
        // Arrange
        var queryPayload = new GetApprovedTemplatesQuery { MaxDurationWeeks = null, SkillId = null };

        // Setup Approved Listing
        var templateApproved = new ProjectTemplate("provider_1", "Cloud Ops Masterclass", "Description lengthy enough to bypass validation parameters.", 12);
        SetPrivateProperty(templateApproved, nameof(ProjectTemplate.Status), ProjectTemplateStatus.Approved);

        // Setup Draft Listing (Should be ignored by search engine)
        var templateDraft = new ProjectTemplate("provider_2", "Secret Pending Core Draft", "Description lengthy enough to bypass validation parameters.", 4);
        SetPrivateProperty(templateDraft, nameof(ProjectTemplate.Status), ProjectTemplateStatus.Draft);

        var templatePool = new List<ProjectTemplate> { templateApproved, templateDraft };
        _mockContext.Setup(c => c.ProjectTemplates).Returns(templatePool.BuildMockDbSet().Object);

        // Act
        var resultList = await _handler.Handle(queryPayload, CancellationToken.None);

        // Assert
        resultList.Should().HaveCount(1);
        resultList.First().Title.Should().Be("Cloud Ops Masterclass");
    }

    [Fact]
    public async Task Handle_MaxDurationFilterSupplied_ShouldFilterOutLongerCurriculums()
    {
        // Arrange
        var queryPayload = new GetApprovedTemplatesQuery { MaxDurationWeeks = 6, SkillId = null };

        var shortTemplate = new ProjectTemplate("provider_1", "Short Crash Course", "Description lengthy enough to bypass validation parameters.", 4);
        SetPrivateProperty(shortTemplate, nameof(ProjectTemplate.Status), ProjectTemplateStatus.Approved);

        var longTemplate = new ProjectTemplate("provider_1", "Long Semester Engine", "Description lengthy enough to bypass validation parameters.", 16);
        SetPrivateProperty(longTemplate, nameof(ProjectTemplate.Status), ProjectTemplateStatus.Approved);

        var templatePool = new List<ProjectTemplate> { shortTemplate, longTemplate };
        _mockContext.Setup(c => c.ProjectTemplates).Returns(templatePool.BuildMockDbSet().Object);

        // Act
        var resultList = await _handler.Handle(queryPayload, CancellationToken.None);

        // Assert
        resultList.Should().HaveCount(1);
        resultList.First().Title.Should().Be("Short Crash Course");
    }

    [Fact]
    public async Task Handle_SkillFilterSupplied_ShouldReturnOnlyTemplatesMatchingSkillId()
    {
        // Arrange
        var targetSearchSkillId = Guid.NewGuid();
        var alternativeSkillId = Guid.NewGuid();
        var queryPayload = new GetApprovedTemplatesQuery { MaxDurationWeeks = null, SkillId = targetSearchSkillId };

        // Template A: Contains the target searching skill badge
        var templateMatching = new ProjectTemplate("provider_1", "Target Tech Track", "Description lengthy enough to bypass validation parameters.", 8);
        SetPrivateProperty(templateMatching, nameof(ProjectTemplate.Id), Guid.NewGuid());
        SetPrivateProperty(templateMatching, nameof(ProjectTemplate.Status), ProjectTemplateStatus.Approved);

        var matchingSkillJoin = new ProjectTemplateSkill(templateMatching.Id, targetSearchSkillId);
        var targetSkillObj = CreateEntityWithPrivateConstructor<Skill>();
        SetPrivateProperty(targetSkillObj, nameof(Skill.Name), "React.js Framework");
        SetPrivateProperty(matchingSkillJoin, nameof(ProjectTemplateSkill.Skill), targetSkillObj);
        SetPrivateProperty(templateMatching, nameof(ProjectTemplate.TemplateSkills), new List<ProjectTemplateSkill> { matchingSkillJoin });

        // Template B: Contains a completely different skill badge
        var templateMismatched = new ProjectTemplate("provider_2", "Alternative Tech Track", "Description lengthy enough to bypass validation parameters.", 8);
        SetPrivateProperty(templateMismatched, nameof(ProjectTemplate.Id), Guid.NewGuid());
        SetPrivateProperty(templateMismatched, nameof(ProjectTemplate.Status), ProjectTemplateStatus.Approved);

        var mismatchedSkillJoin = new ProjectTemplateSkill(templateMismatched.Id, alternativeSkillId);
        SetPrivateProperty(templateMismatched, nameof(ProjectTemplate.TemplateSkills), new List<ProjectTemplateSkill> { mismatchedSkillJoin });

        var templatePool = new List<ProjectTemplate> { templateMatching, templateMismatched };
        _mockContext.Setup(c => c.ProjectTemplates).Returns(templatePool.BuildMockDbSet().Object);

        // Act
        var resultList = await _handler.Handle(queryPayload, CancellationToken.None);

        // Assert
        resultList.Should().HaveCount(1);
        resultList.First().Title.Should().Be("Target Tech Track");
        resultList.First().Skills.First().Name.Should().Be("React.js Framework");
    }

    private static T CreateEntityWithPrivateConstructor<T>() =>
        (T)Activator.CreateInstance(typeof(T), true)!;

    private static void SetPrivateProperty(object obj, string propertyName, object value) =>
        obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?
           .SetValue(obj, value);
}
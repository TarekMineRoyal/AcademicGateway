using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.Skills.Queries.GetSkills;
using AcademicGateway.Domain.Skills;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Skills.Queries.GetSkills;

/// <summary>
/// Contains isolated unit verification routines for the <see cref="GetSkillsQueryHandler"/>.
/// Validates relational read-only global lookups, entity-to-DTO data mapping projections, 
/// and empty data boundary collection pathways.
/// </summary>
public class GetSkillsQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly GetSkillsQueryHandler _handler;

    /// <summary>
    /// Initializes a pristine instance of the test class with isolated database mock mappings.
    /// </summary>
    public GetSkillsQueryHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _handler = new GetSkillsQueryHandler(_dbContextMock.Object);
    }

    /// <summary>
    /// Assures that executing the global lookup query correctly builds and returns an untracked, 
    /// mapped collection of all professional and technical skills available within the system registry.
    /// </summary>
    [Fact]
    public async Task Handle_WhenSkillsExistInRegistry_ShouldReturnAllProjectedSkillDtos()
    {
        // Arrange
        var skill1 = new Skill("C#");
        var skill2 = new Skill("Python");

        var skillsList = new List<Skill> { skill1, skill2 };

        // BuildMockDbSet maps our in-memory list to simulate an asynchronous queryable Entity Framework dataset
        var mockDbSet = skillsList.BuildMockDbSet();
        _dbContextMock.Setup(db => db.Skills).Returns(mockDbSet.Object);

        var query = new GetSkillsQuery();

        // Act
        // Best Practice (xUnit1051): Pass TestContext.Current.CancellationToken to ensure prompt test runner cancellation responses.
        var resultList = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        resultList.Should().NotBeNull();
        resultList.Should().HaveCount(2);

        // Confirm that the LINQ projection successfully bound both the domain entity text values and tracking Guids
        resultList.Should().ContainSingle(s => s.Id == skill1.Id && s.Name == "C#");
        resultList.Should().ContainSingle(s => s.Id == skill2.Id && s.Name == "Python");
    }

    /// <summary>
    /// Assures that when no skills have been populated or configured within the institutional directory registry,
    /// the query executes cleanly without errors and returns an empty read-only collection contract wrapper.
    /// </summary>
    [Fact]
    public async Task Handle_WhenNoSkillsExistInRegistry_ShouldReturnEmptyCollectionFlawlessly()
    {
        // Arrange
        var emptySkillsList = new List<Skill>();
        var mockDbSet = emptySkillsList.BuildMockDbSet();

        // Mock the db table to return an empty queryable data baseline
        _dbContextMock.Setup(db => db.Skills).Returns(mockDbSet.Object);

        var query = new GetSkillsQuery();

        // Act
        var resultList = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        resultList.Should().NotBeNull();
        resultList.Should().BeEmpty();
    }
}
using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.Curriculum.Queries.GetMajorsWithSpecialties;
using AcademicGateway.Domain.Curriculum;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Curriculum.Queries.GetMajorsWithSpecialties;

/// <summary>
/// Contains unit tests executing isolation routines for the <see cref="GetMajorsWithSpecialtiesQueryHandler"/>.
/// Verifies high-performance untracked relational LINQ projections, empty dataset thresholds,
/// zero child subquery projections, and independent multi-row collection mapping integrity.
/// </summary>
public class GetMajorsWithSpecialtiesQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly GetMajorsWithSpecialtiesQueryHandler _handler;

    /// <summary>
    /// Initializes a pristine instance of the test class with isolated database mock mappings.
    /// </summary>
    public GetMajorsWithSpecialtiesQueryHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _handler = new GetMajorsWithSpecialtiesQueryHandler(_dbContextMock.Object);
    }

    /// <summary>
    /// Assures that executing the global curriculum query maps database entries straight out of 
    /// domain models into projected data transfer objects, matching titles and tracking keys.
    /// </summary>
    [Fact]
    public async Task Handle_GivenPopulatedCurriculumRegistry_ShouldReturnAllProjectedMajorsAndSpecialties()
    {
        // Arrange
        // Best Practice: Instantiate entities via standard domain constructor routing and behavior methods.
        // This honors the private setters and ensures internal IDs generate gracefully inside aggregate roots.
        var major = new Major("Computer Science");
        major.AddSpecialty("Software Engineering");

        var majorsList = new List<Major> { major };

        // BuildMockDbSet maps our in-memory list to simulate an asynchronous queryable Entity Framework dataset
        var mockDbSet = majorsList.BuildMockDbSet();
        _dbContextMock.Setup(db => db.Majors).Returns(mockDbSet.Object);

        var query = new GetMajorsWithSpecialtiesQuery();

        // Act
        // Best Practice (xUnit1051): Use TestContext.Current.CancellationToken for responsive test run abort controls.
        var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);

        // Target individual entries safely via LINQ extensions since IReadOnlyCollection doesn't expose standard indexers
        var projectedMajor = result.First();
        projectedMajor.Id.Should().Be(major.Id);
        projectedMajor.Name.Should().Be("Computer Science");

        projectedMajor.Specialties.Should().HaveCount(1);

        var projectedSpecialty = projectedMajor.Specialties.First();
        var domainSpecialty = major.Specialties.First();

        projectedSpecialty.Id.Should().Be(domainSpecialty.Id);
        projectedSpecialty.Name.Should().Be("Software Engineering");
    }

    /// <summary>
    /// Assures that when no academic tracks have been configured or seeded within the system database,
    /// the handler runs cleanly without errors and returns an empty read-only collection wrapper.
    /// </summary>
    [Fact]
    public async Task Handle_GivenEmptyMajorsRegistry_ShouldReturnEmptyCollectionSafely()
    {
        // Arrange
        var emptyList = new List<Major>();
        var mockDbSet = emptyList.BuildMockDbSet();
        _dbContextMock.Setup(db => db.Majors).Returns(mockDbSet.Object);

        var query = new GetMajorsWithSpecialtiesQuery();

        // Act
        var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Assures that if an academic major exists but has not been assigned any child tracks yet,
    /// the EF projection maps successfully, leaving the Specialties sub-list empty rather than causing errors.
    /// </summary>
    [Fact]
    public async Task Handle_GivenMajorWithZeroSpecialties_ShouldProjectMajorWithEmptySpecialtiesCollection()
    {
        // Arrange
        // Precondition: Construct the parent major but do NOT execute .AddSpecialty() routines.
        var bareMajor = new Major("Historical Literature");

        var majorsList = new List<Major> { bareMajor };
        var mockDbSet = majorsList.BuildMockDbSet();
        _dbContextMock.Setup(db => db.Majors).Returns(mockDbSet.Object);

        var query = new GetMajorsWithSpecialtiesQuery();

        // Act
        var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);

        var projectedItem = result.First();
        projectedItem.Name.Should().Be("Historical Literature");
        projectedItem.Specialties.Should().NotBeNull();
        projectedItem.Specialties.Should().BeEmpty();
    }

    /// <summary>
    /// Assures that when multiple distinct major entities are processed by the query, the LINQ subquery
    /// projection accurately isolates child rows to prevent cross-contamination or leaking properties.
    /// </summary>
    [Fact]
    public async Task Handle_GivenMultipleMajorsInRegistry_ShouldAccuratelyIsolateAndProjectAllIndependentStructures()
    {
        // Arrange
        var majorA = new Major("Business Management");
        majorA.AddSpecialty("Corporate Finance");

        var majorB = new Major("Mechanical Engineering");
        majorB.AddSpecialty("Fluid Dynamics");

        var multiMajorPool = new List<Major> { majorA, majorB };
        var mockDbSet = multiMajorPool.BuildMockDbSet();
        _dbContextMock.Setup(db => db.Majors).Returns(mockDbSet.Object);

        var query = new GetMajorsWithSpecialtiesQuery();

        // Act
        var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        // Deep verify that Major A targets only its explicit child components
        var projectedA = result.Single(m => m.Id == majorA.Id);
        projectedA.Name.Should().Be("Business Management");
        projectedA.Specialties.Should().ContainSingle(s => s.Name == "Corporate Finance");

        // Deep verify that Major B targets only its explicit child components
        var projectedB = result.Single(m => m.Id == majorB.Id);
        projectedB.Name.Should().Be("Mechanical Engineering");
        projectedB.Specialties.Should().ContainSingle(s => s.Name == "Fluid Dynamics");
    }
}
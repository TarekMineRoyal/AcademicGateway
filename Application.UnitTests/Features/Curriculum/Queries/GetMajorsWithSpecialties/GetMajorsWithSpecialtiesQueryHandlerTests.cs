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
/// Unit tests executing isolation routines for the <see cref="GetMajorsWithSpecialtiesQueryHandler"/>.
/// Verifies high-performance untracked relational projection properties map accurately out of domain models.
/// </summary>
public class GetMajorsWithSpecialtiesQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly GetMajorsWithSpecialtiesQueryHandler _handler;

    public GetMajorsWithSpecialtiesQueryHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _handler = new GetMajorsWithSpecialtiesQueryHandler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_All_Majors_With_Their_Specialties()
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
        var result = await _handler.Handle(query, CancellationToken.None);

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
}
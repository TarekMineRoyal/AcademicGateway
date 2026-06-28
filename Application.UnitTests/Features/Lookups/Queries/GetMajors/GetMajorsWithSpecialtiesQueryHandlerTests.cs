using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.Lookups.Queries.GetMajors;
using AcademicGateway.Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Lookups.Queries.GetMajors;

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
        var majorId = Guid.NewGuid();
        var specialtyId = Guid.NewGuid();

        var majors = new List<Major>
        {
            new Major
            {
                Id = majorId,
                Name = "Computer Science",
                Specialties = new List<Specialty>
                {
                    new Specialty { Id = specialtyId, MajorId = majorId, Name = "Software Engineering" }
                }
            }
        };

        // BuildMockDbSet maps our in-memory list to simulate a queryable Entity Framework entity collection
        var mockDbSet = majors.BuildMockDbSet();
        _dbContextMock.Setup(db => db.Majors).Returns(mockDbSet.Object);

        var query = new GetMajorsWithSpecialtiesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(majorId);
        result[0].Name.Should().Be("Computer Science");
        result[0].Specialties.Should().HaveCount(1);
        result[0].Specialties[0].Id.Should().Be(specialtyId);
        result[0].Specialties[0].Name.Should().Be("Software Engineering");
    }
}
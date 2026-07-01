using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.Lookups.Queries.GetSkills;
using Domain.Lookups;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Lookups.Queries.GetSkills;

public class GetSkillsQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly GetSkillsQueryHandler _handler;

    public GetSkillsQueryHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _handler = new GetSkillsQueryHandler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_All_Skills()
    {
        // Arrange
        var skills = new List<Skill>
        {
            new Skill { Id = Guid.NewGuid(), Name = "C#" },
            new Skill { Id = Guid.NewGuid(), Name = "Python" }
        };

        var mockDbSet = skills.BuildMockDbSet();
        _dbContextMock.Setup(db => db.Skills).Returns(mockDbSet.Object);

        var query = new GetSkillsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().ContainSingle(s => s.Name == "C#");
        result.Should().ContainSingle(s => s.Name == "Python");
    }
}
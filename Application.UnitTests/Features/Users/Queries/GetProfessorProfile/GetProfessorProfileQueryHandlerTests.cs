using AcademicGateway.Application.Common.Interfaces;
using Application.Features.Professors.Queries.GetProfessorProfile;
using Domain.Professors;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Users.Queries.GetProfessorProfile;

public class GetProfessorProfileQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly GetProfessorProfileQueryHandler _handler;

    public GetProfessorProfileQueryHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _handler = new GetProfessorProfileQueryHandler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Profile_When_Professor_Exists()
    {
        // Arrange
        var targetUserId = "prof-789";
        var professors = new List<Professor>
        {
            new Professor { UserId = targetUserId, AcademicDepartment = "Mathematics" }
        };

        var mockDbSet = professors.BuildMockDbSet();
        _dbContextMock.Setup(db => db.Professors).Returns(mockDbSet.Object);

        var query = new GetProfessorProfileQuery(targetUserId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(targetUserId);
        result.AcademicDepartment.Should().Be("Mathematics");
    }

    [Fact]
    public async Task Handle_Should_Throw_KeyNotFoundException_When_Professor_Does_Not_Exist()
    {
        // Arrange
        var mockDbSet = new List<Professor>().BuildMockDbSet();
        _dbContextMock.Setup(db => db.Professors).Returns(mockDbSet.Object);

        var query = new GetProfessorProfileQuery("wrong-id");

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*User ID 'wrong-id' was not found.*");
    }
}
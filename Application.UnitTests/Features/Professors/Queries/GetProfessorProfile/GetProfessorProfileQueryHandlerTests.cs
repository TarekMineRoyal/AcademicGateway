using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.Professors.Queries.GetProfessorProfile;
using AcademicGateway.Domain.Professors;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Professors.Queries.GetProfessorProfile;

/// <summary>
/// Contains isolated unit verification routines for the <see cref="GetProfessorProfileQueryHandler"/>.
/// Validates relational read-only projections, domain model to DTO mappings, and lookup exception handling.
/// </summary>
public class GetProfessorProfileQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly GetProfessorProfileQueryHandler _handler;

    /// <summary>
    /// Initializes a new instance of the test class, establishing isolated mock database contexts.
    /// </summary>
    public GetProfessorProfileQueryHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _handler = new GetProfessorProfileQueryHandler(_dbContextMock.Object);
    }

    /// <summary>
    /// Assures that querying for an existing professor profile accurately builds and transfers 
    /// a projected <see cref="ProfessorProfileDto"/> complete with all rich analytics and core details.
    /// </summary>
    [Fact]
    public async Task Handle_GivenExistingProfessorId_ShouldReturnProjectedProfileDto()
    {
        // Arrange
        var targetProfessorId = Guid.NewGuid();

        // Best Practice: Populate via domain constructor rules to fulfill aggregate validation requirements.
        var professor = new Professor(
            id: targetProfessorId,
            fullName: "Dr. Alice Smith",
            department: "Mathematics",
            rank: "Full Professor",
            maxSupervisionCapacity: 6
        );

        var professorsList = new List<Professor> { professor };
        var mockDbSet = professorsList.BuildMockDbSet();
        _dbContextMock.Setup(db => db.Professors).Returns(mockDbSet.Object);

        var query = new GetProfessorProfileQuery(targetProfessorId);

        // Act
        // Best Practice (xUnit1051): Pass TestContext.Current.CancellationToken to ensure prompt cancellation response.
        var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(targetProfessorId);
        result.FullName.Should().Be("Dr. Alice Smith");
        result.AcademicDepartment.Should().Be("Mathematics");
        result.Rank.Should().Be("Full Professor");
        result.MaxSupervisionCapacity.Should().Be(6);
        result.CurrentProjectCount.Should().Be(0);
        result.IsAcceptingProjects.Should().BeTrue();
    }

    /// <summary>
    /// Assures that looking up a missing or invalid professor identification code 
    /// securely breaks execution and throws a comprehensive <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNonExistentProfessorId_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var mockDbSet = new List<Professor>().BuildMockDbSet();
        _dbContextMock.Setup(db => db.Professors).Returns(mockDbSet.Object);

        var wrongId = Guid.NewGuid();
        var query = new GetProfessorProfileQuery(wrongId);

        // Act
        // Best Practice (xUnit1051): Pass TestContext.Current.CancellationToken inside the executing delegate.
        Func<Task> act = async () => await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*Professor profile for ID '{wrongId}' was not found.*");
    }
}
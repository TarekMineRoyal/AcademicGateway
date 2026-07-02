using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.Professors.Commands.RegisterProfessor;
using AcademicGateway.Domain.Professors;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Professors.Commands.RegisterProfessor;

/// <summary>
/// Contains isolated unit verification routines for the <see cref="RegisterProfessorCommandHandler"/>.
/// Validates user credential identity handoffs, aggregate domain hydration, and storage persistence.
/// </summary>
public class RegisterProfessorCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly RegisterProfessorCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the test class, establishing isolated mock dependencies.
    /// </summary>
    public RegisterProfessorCommandHandlerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _dbContextMock = new Mock<IApplicationDbContext>();

        // Setup the mock for the Professors table to simulate an empty relational data sequence
        var mockProfessorsDbSet = new List<Professor>().BuildMockDbSet();
        _dbContextMock.Setup(db => db.Professors).Returns(mockProfessorsDbSet.Object);

        _handler = new RegisterProfessorCommandHandler(_identityServiceMock.Object, _dbContextMock.Object);
    }

    /// <summary>
    /// Assures that passing a valid registration specification successfully yields the assigned identity Guid
    /// and queues a fully populated Professor aggregate root structure for relational database persistence.
    /// </summary>
    [Fact]
    public async Task Handle_GivenValidRegistrationDetails_ShouldReturnUserIdAndPersistProfile()
    {
        // Arrange
        var command = new RegisterProfessorCommand
        {
            Username = "professor_smith",
            Email = "prof@university.edu",
            Password = "SecurePassword123!",
            FullName = "Dr. John Smith",        // Required by aggregate invariant constraints
            AcademicDepartment = "Computer Science",
            Rank = "Associate Professor",        // Required by aggregate invariant constraints
            MaxSupervisionCapacity = 5           // Must be greater than 0 to pass internal guard rules
        };

        var expectedUserId = Guid.NewGuid();

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((true, expectedUserId, new List<string>()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedUserId);

        // Verify the Professor entity was mapped and queued with correct parameters inside DbContext
        _dbContextMock.Verify(x => x.Professors.Add(It.Is<Professor>(p =>
            p.Id == expectedUserId &&
            p.FullName == command.FullName &&
            p.Department == command.AcademicDepartment &&
            p.Rank == command.Rank &&
            p.MaxSupervisionCapacity == command.MaxSupervisionCapacity)), Times.Once);

        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Assures that if credentials provisioning fails at the infrastructure identity layer,
    /// an <see cref="InvalidOperationException"/> is thrown and data transactions are aborted.
    /// </summary>
    [Fact]
    public async Task Handle_GivenIdentityServiceFailure_ShouldThrowInvalidOperationExceptionAndAbortPersistence()
    {
        // Arrange
        var command = new RegisterProfessorCommand
        {
            Username = "professor_smith",
            Email = "prof@university.edu",
            Password = "SecurePassword123!",
            FullName = "Dr. John Smith",
            AcademicDepartment = "Computer Science",
            Rank = "Associate Professor",
            MaxSupervisionCapacity = 5
        };

        var errors = new List<string> { "Password is too weak" };

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((false, Guid.Empty, errors));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Professor credential provisioning failed*Password is too weak*");

        // Ensure we never try to commit or flush storage changes when identity verification faults
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
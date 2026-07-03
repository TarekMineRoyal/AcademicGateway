using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.Professors.Commands.RegisterProfessor;
using AcademicGateway.Domain.Professors;
using AcademicGateway.Domain.Professors.Exceptions;
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
/// Validates user credential identity handoffs, aggregate domain hydration invariants, 
/// capacity threshold boundaries, empty text failures, and transactional persistence.
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

        // Setup the mock for the Professors table to simulate an empty relational data sequence baseline
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
            FullName = "Dr. John Smith",
            AcademicDepartment = "Computer Science",
            Rank = "Associate Professor",
            MaxSupervisionCapacity = 5
        };

        var expectedUserId = Guid.NewGuid();

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((true, expectedUserId, new List<string>()));

        // Act
        // Best Practice (xUnit1051): Pass TestContext.Current.CancellationToken for responsive test abort controls.
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(expectedUserId);

        // Verify the Professor entity was mapped and queued with correct parameters inside DbContext
        _dbContextMock.Verify(x => x.Professors.Add(It.Is<Professor>(p =>
            p.Id == expectedUserId &&
            p.FullName == command.FullName &&
            p.Department == command.AcademicDepartment &&
            p.Rank == command.Rank &&
            p.MaxSupervisionCapacity == command.MaxSupervisionCapacity)), Times.Once);

        _dbContextMock.Verify(x => x.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
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
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Professor credential provisioning failed*Password is too weak*");

        // Ensure we never try to commit or flush storage changes when identity verification faults
        _dbContextMock.Verify(x => x.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Never);
    }

    /// <summary>
    /// Assures that if the identity tier reports a successful user allocation but returns an empty Guid tracking key,
    /// the aggregate root intercepts execution, throwing an <see cref="InvalidProfessorDetailsException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_GivenIdentitySucceedsWithEmptyGuid_ShouldPropagateInvalidProfessorDetailsExceptionAndAbort()
    {
        // Arrange
        var command = new RegisterProfessorCommand
        {
            Username = "emptyguidprof",
            Email = "empty@uni.edu",
            Password = "Password123!",
            FullName = "Dr. Blank Identity",
            AcademicDepartment = "Data Science",
            Rank = "Assistant Professor",
            MaxSupervisionCapacity = 2
        };

        // Violation: succeeded is true but returned unique user token resolves to Guid.Empty
        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((true, Guid.Empty, new List<string>()));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidProfessorDetailsException>();
        _dbContextMock.Verify(x => x.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Never);
    }

    /// <summary>
    /// Assures that if an invalid, empty, or whitespace display name string parameter is passed to the handler,
    /// deep domain validation rules prevent instantiation, throwing an <see cref="InvalidProfessorDetailsException"/>.
    /// Note: Parameter defined as string? to cleanly eliminate analyzer reference variable check warnings.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("     ")]
    [InlineData(null)]
    public async Task Handle_GivenInvalidFullName_ShouldPropagateInvalidProfessorDetailsExceptionAndAbort(string? invalidName)
    {
        // Arrange
        var command = new RegisterProfessorCommand
        {
            Username = "badprofname",
            Email = "badname@uni.edu",
            Password = "Password123!",
            FullName = invalidName!,
            AcademicDepartment = "Engineering",
            Rank = "Full Professor",
            MaxSupervisionCapacity = 4
        };

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((true, Guid.NewGuid(), new List<string>()));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidProfessorDetailsException>();
        _dbContextMock.Verify(x => x.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Never);
    }

    /// <summary>
    /// Assures that if an empty or whitespace academic department tracking title is supplied,
    /// the domain layer blocks construction, throwing an <see cref="InvalidProfessorDetailsException"/>.
    /// Note: Parameter defined as string? to cleanly eliminate analyzer reference variable check warnings.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" \n \t ")]
    [InlineData(null)]
    public async Task Handle_GivenInvalidAcademicDepartment_ShouldPropagateInvalidProfessorDetailsExceptionAndAbort(string? invalidDept)
    {
        // Arrange
        var command = new RegisterProfessorCommand
        {
            Username = "badprofdept",
            Email = "baddept@uni.edu",
            Password = "Password123!",
            FullName = "Dr. Valid Name",
            AcademicDepartment = invalidDept!,
            Rank = "Full Professor",
            MaxSupervisionCapacity = 4
        };

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((true, Guid.NewGuid(), new List<string>()));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidProfessorDetailsException>();
        _dbContextMock.Verify(x => x.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Never);
    }

    /// <summary>
    /// Assures that if an empty or whitespace institutional rank descriptor phrase is supplied,
    /// the domain layer blocks construction, throwing an <see cref="InvalidProfessorDetailsException"/>.
    /// Note: Parameter defined as string? to cleanly eliminate analyzer reference variable check warnings.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("     ")]
    [InlineData(null)]
    public async Task Handle_GivenInvalidRank_ShouldPropagateInvalidProfessorDetailsExceptionAndAbort(string? invalidRank)
    {
        // Arrange
        var command = new RegisterProfessorCommand
        {
            Username = "badprofrank",
            Email = "badrank@uni.edu",
            Password = "Password123!",
            FullName = "Dr. Valid Name",
            AcademicDepartment = "Mathematics",
            Rank = invalidRank!,
            MaxSupervisionCapacity = 4
        };

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((true, Guid.NewGuid(), new List<string>()));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidProfessorDetailsException>();
        _dbContextMock.Verify(x => x.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Never);
    }

    /// <summary>
    /// Assures that when the requested maximum student supervision capability count drops below system threshold bounds
    /// (such as zero or negative spaces), the aggregate blocks generation by throwing an <see cref="InvalidSupervisionCapacityException"/>.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Handle_GivenInvalidSupervisionCapacityBounds_ShouldPropagateInvalidSupervisionCapacityExceptionAndAbort(int invalidCapacity)
    {
        // Arrange
        var command = new RegisterProfessorCommand
        {
            Username = "badcapacityprof",
            Email = "capacity@uni.edu",
            Password = "Password123!",
            FullName = "Dr. John Doe",
            AcademicDepartment = "Computer Science",
            Rank = "Assistant Professor",
            MaxSupervisionCapacity = invalidCapacity // Violation: Values must register above 0
        };

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((true, Guid.NewGuid(), new List<string>()));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidSupervisionCapacityException>();
        _dbContextMock.Verify(x => x.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Never);
    }
}
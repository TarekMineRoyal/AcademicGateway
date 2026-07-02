using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.Students.Commands.RegisterStudent;
using AcademicGateway.Domain.Students;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Students.Commands.RegisterStudent;

/// <summary>
/// Contains isolated unit verification routines for the <see cref="RegisterStudentCommandHandler"/>.
/// Validates secure identity service calls, aggregate root initialization invariants, and storage persistence.
/// </summary>
public class RegisterStudentCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly RegisterStudentCommandHandler _handler;

    /// <summary>
    /// Initializes a pristine instance of the test class, establishing isolated mock dependencies.
    /// </summary>
    public RegisterStudentCommandHandlerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _dbContextMock = new Mock<IApplicationDbContext>();

        // Setup the mock for the Students table to simulate an empty relational dataset sequence baseline
        var mockStudentsDbSet = new List<Student>().BuildMockDbSet();
        _dbContextMock.Setup(db => db.Students).Returns(mockStudentsDbSet.Object);

        _handler = new RegisterStudentCommandHandler(_identityServiceMock.Object, _dbContextMock.Object);
    }

    /// <summary>
    /// Assures that passing a valid student registration specification successfully provisions identity 
    /// credentials, instantiates a corresponding rich Student aggregate root, and saves the changes.
    /// </summary>
    [Fact]
    public async Task Handle_GivenValidRegistrationDetails_ShouldReturnUserIdAndPersistProfile()
    {
        // Arrange
        var command = new RegisterStudentCommand
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!",
            FullName = "John Doe", // Required to pass constructor aggregate invariants
            GraduationYear = 2025,
            MajorIds = new List<Guid> { Guid.NewGuid() }
        };

        var expectedUserId = Guid.NewGuid();

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((true, expectedUserId, new List<string>()));

        // Act
        // Best Practice (xUnit1051): Pass TestContext.Current.CancellationToken to allow responsive test run controls.
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(expectedUserId);

        // Verify that the Student aggregate root was tracked and added using the correct mapped identity properties
        _dbContextMock.Verify(x => x.Students.Add(It.Is<Student>(s =>
            s.Id == expectedUserId &&
            s.FullName == command.FullName &&
            s.GraduationYear == command.GraduationYear)), Times.Once);

        // Confirm database transaction persistence integrity across the unit of work
        _dbContextMock.Verify(x => x.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Assures that if credentials provisioning fails at the infrastructure identity tier,
    /// an <see cref="InvalidOperationException"/> is thrown and data mutations are entirely aborted.
    /// </summary>
    [Fact]
    public async Task Handle_GivenIdentityServiceFailure_ShouldThrowInvalidOperationExceptionAndAbortPersistence()
    {
        // Arrange
        var command = new RegisterStudentCommand
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!",
            FullName = "John Doe"
        };

        var errors = new List<string> { "Email already taken" };

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((false, Guid.Empty, errors));

        // Act
        // Wrap execution inside a delegate stream to allow FluentAssertions to track chronological exceptions safely
        Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Student identity configuration failed*Email already taken*");

        // Guarantee that no tracking alterations or uncommitted records are flushed to persistence
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
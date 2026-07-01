using AcademicGateway.Application.Common.Interfaces;
using Application.Features.Students.Commands.RegisterStudent;
using Domain.Students;
using FluentAssertions; // We use this for clean Assertions!
using MockQueryable.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Users.Commands.RegisterStudent;

public class RegisterStudentCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly RegisterStudentCommandHandler _handler;

    public RegisterStudentCommandHandlerTests()
    {
        // 1. Create fake versions of the handler's dependencies
        _identityServiceMock = new Mock<IIdentityService>();
        _dbContextMock = new Mock<IApplicationDbContext>();

        // 2. Setup the fake DbContext so that accessing .Students doesn't throw a null exception
        var mockStudentsDbSet = new List<Student>().BuildMockDbSet();
        _dbContextMock.Setup(db => db.Students).Returns(mockStudentsDbSet.Object);

        // 3. Inject the fakes into the handler
        _handler = new RegisterStudentCommandHandler(_identityServiceMock.Object, _dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_UserId_On_Successful_Registration()
    {
        // Arrange
        var command = new RegisterStudentCommand
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!",
            GraduationYear = 2025,
            MajorIds = new List<Guid> { Guid.NewGuid() }
        };

        var expectedUserId = Guid.NewGuid().ToString();

        // Tell our fake IdentityService to return "Success" and our fake UserId when called
        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((true, expectedUserId, new List<string>()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Did it return the expected ID?
        result.Should().Be(expectedUserId);

        // Did it add exactly one student to the Students table with the correct ID?
        _dbContextMock.Verify(x => x.Students.Add(It.Is<Student>(s => s.UserId == expectedUserId)), Times.Once);

        // Did it actually save the database changes?
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_Exception_When_Identity_Creation_Fails()
    {
        // Arrange
        var command = new RegisterStudentCommand
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!"
        };

        var errors = new List<string> { "Email already taken" };

        // Tell our fake IdentityService to fail and return our error message
        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((false, string.Empty, errors));

        // Act
        // We capture the action so we can assert that it throws an exception
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Did it throw an exception containing our error message?
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*User creation failed*Email already taken*");

        // IMPORTANT: Ensure the database was NEVER called to save anything if identity failed
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
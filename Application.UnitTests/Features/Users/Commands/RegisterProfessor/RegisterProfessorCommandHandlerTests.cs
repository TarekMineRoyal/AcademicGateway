using AcademicGateway.Application.Common.Interfaces;
using Application.Features.Professors.Commands.RegisterProfessor;
using Domain.Professors;
using FluentAssertions;
using MockQueryable.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Users.Commands.RegisterProfessor;

public class RegisterProfessorCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly RegisterProfessorCommandHandler _handler;

    public RegisterProfessorCommandHandlerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _dbContextMock = new Mock<IApplicationDbContext>();

        // Setup the mock for the Professors table
        var mockProfessorsDbSet = new List<Professor>().BuildMockDbSet();
        _dbContextMock.Setup(db => db.Professors).Returns(mockProfessorsDbSet.Object);

        _handler = new RegisterProfessorCommandHandler(_identityServiceMock.Object, _dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_UserId_On_Successful_Registration()
    {
        // Arrange
        var command = new RegisterProfessorCommand
        {
            Username = "professor_smith",
            Email = "prof@university.edu",
            Password = "SecurePassword123!",
            AcademicDepartment = "Computer Science"
        };

        var expectedUserId = Guid.NewGuid().ToString();

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((true, expectedUserId, new List<string>()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedUserId);

        // Verify the Professor entity was added with the correct mapping
        _dbContextMock.Verify(x => x.Professors.Add(It.Is<Professor>(p =>
            p.UserId == expectedUserId &&
            p.AcademicDepartment == command.AcademicDepartment)), Times.Once);

        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_Exception_When_Identity_Creation_Fails()
    {
        // Arrange
        var command = new RegisterProfessorCommand
        {
            Username = "professor_smith",
            Email = "prof@university.edu",
            Password = "SecurePassword123!"
        };

        var errors = new List<string> { "Password is too weak" };

        _identityServiceMock
            .Setup(x => x.CreateUserAsync(command.Username, command.Email, command.Password))
            .ReturnsAsync((false, string.Empty, errors));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Professor creation failed*Password is too weak*");

        // Ensure we never try to save a partially created profile
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
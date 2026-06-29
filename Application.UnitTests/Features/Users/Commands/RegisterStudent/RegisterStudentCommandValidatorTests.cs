using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.Users.Commands.RegisterStudent;
using AcademicGateway.Domain.Entities;
using FluentValidation.TestHelper;
using MockQueryable.EntityFrameworkCore; // Added this!
using MockQueryable.Moq;
using Moq;
using Xunit; // Added this for TestContext!

namespace AcademicGateway.Application.UnitTests.Features.Users.Commands.RegisterStudent;

public class RegisterStudentCommandValidatorTests
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly RegisterStudentCommandValidator _validator;

    public RegisterStudentCommandValidatorTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _validator = new RegisterStudentCommandValidator(_dbContextMock.Object);
    }

    [Fact]
    public async Task Should_Have_Error_When_Email_Is_Empty()
    {
        // Arrange
        var command = new RegisterStudentCommand { Email = string.Empty };

        // Act - Passed the cancellation token to satisfy xUnit
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Should_Have_Error_When_Password_Is_Too_Short()
    {
        // Arrange
        var command = new RegisterStudentCommand { Password = "123" };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password must be at least 8 characters long.");
    }

    [Fact]
    public async Task Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var majorId = Guid.NewGuid();
        var specialtyId = Guid.NewGuid();

        var specialties = new List<Specialty>
        {
            new Specialty { Id = specialtyId, MajorId = majorId, Name = "Test Specialty" }
        };

        // Notice we removed .AsQueryable() - the new package extends IEnumerable directly!
        var mockDbSet = specialties.BuildMockDbSet();

        _dbContextMock.Setup(x => x.Specialties).Returns(mockDbSet.Object);

        var command = new RegisterStudentCommand
        {
            Email = "valid@example.com",
            Username = "validuser",
            Password = "ValidPassword123!",
            MajorIds = new List<Guid> { majorId },
            SpecialtyIds = new List<Guid> { specialtyId }
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
using AcademicGateway.Application.Features.Users.Commands.RegisterProfessor;
using FluentValidation.TestHelper;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Users.Commands.RegisterProfessor;

public class RegisterProfessorCommandValidatorTests
{
    private readonly RegisterProfessorCommandValidator _validator;

    public RegisterProfessorCommandValidatorTests()
    {
        _validator = new RegisterProfessorCommandValidator();
    }

    [Fact]
    public async Task Should_Have_Error_When_AcademicDepartment_Is_Empty()
    {
        // Arrange
        var command = new RegisterProfessorCommand { AcademicDepartment = string.Empty };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AcademicDepartment)
              .WithErrorMessage("Academic department is required.");
    }

    [Fact]
    public async Task Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new RegisterProfessorCommand
        {
            Email = "prof@university.edu",
            Username = "professor_smith",
            Password = "SecurePassword123!",
            AcademicDepartment = "Computer Science"
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
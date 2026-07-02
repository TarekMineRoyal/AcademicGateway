using AcademicGateway.Application.Features.Professors.Commands.RegisterProfessor;
using FluentValidation.TestHelper;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Professors.Commands.RegisterProfessor;

/// <summary>
/// Contains unit tests for the <see cref="RegisterProfessorCommandValidator"/>.
/// Evaluates input validation logic, length boundaries, and rule guard messages before handler execution.
/// </summary>
public class RegisterProfessorCommandValidatorTests
{
    private readonly RegisterProfessorCommandValidator _validator;

    /// <summary>
    /// Initializes a pristine instance of the validator under evaluation.
    /// </summary>
    public RegisterProfessorCommandValidatorTests()
    {
        _validator = new RegisterProfessorCommandValidator();
    }

    /// <summary>
    /// Assures that if an incoming request provides an empty or whitespace-only 
    /// Academic Department string, a precise validation error is logged.
    /// </summary>
    [Fact]
    public async Task Validate_GivenEmptyAcademicDepartment_ShouldHaveValidationErrorWithPreciseMessage()
    {
        // Arrange
        var command = new RegisterProfessorCommand
        {
            AcademicDepartment = string.Empty
        };

        // Act
        // Best Practice (xUnit1051): Pass TestContext.Current.CancellationToken to allow responsive test aborting.
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AcademicDepartment)
              .WithErrorMessage("Academic department assignment details cannot be empty or whitespace.");
    }

    /// <summary>
    /// Assures that when all required fields satisfy formatting rules, length criteria, 
    /// and threshold rules, the validator passes cleanly with no errors.
    /// </summary>
    [Fact]
    public async Task Validate_GivenValidCommandConfiguration_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        // Best Practice: Fully populate all properties that carry validation constraints 
        // to prevent false negatives from unrelated missing fields.
        var command = new RegisterProfessorCommand
        {
            Email = "prof@university.edu",
            Username = "professor_smith",
            Password = "SecurePassword123!",
            FullName = "Dr. Jane Smith",
            AcademicDepartment = "Computer Science",
            Rank = "Full Professor",
            MaxSupervisionCapacity = 4 // Must exceed zero to clear capacity rules
        };

        // Act
        // Best Practice (xUnit1051): Pass TestContext.Current.CancellationToken to allow responsive test aborting.
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
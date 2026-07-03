using AcademicGateway.Application.Features.Professors.Commands.RegisterProfessor;
using FluentValidation.TestHelper;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Professors.Commands.RegisterProfessor;

/// <summary>
/// Contains isolated unit tests for the <see cref="RegisterProfessorCommandValidator"/>.
/// Evaluates input validation logic, identity extension rules, text layout constraints, 
/// character length limits, and capacity numeric boundary errors.
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
    /// Assures that when all required fields satisfy formatting rules, length criteria, 
    /// and threshold rules, the validator passes cleanly with no errors.
    /// </summary>
    [Fact]
    public async Task Validate_GivenValidCommandConfiguration_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
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

    /// <summary>
    /// Assures that if an incoming command request features a missing, null, or whitespace-only Full Name,
    /// a precise validation failure is successfully captured and logged with the correct error payload.
    /// Note: Parameter defined as string? to cleanly eliminate analyzer type warnings.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("     ")]
    [InlineData(null)]
    public async Task Validate_GivenInvalidFullName_ShouldHaveValidationErrorWithPreciseMessage(string? invalidName)
    {
        // Arrange
        var command = new RegisterProfessorCommand
        {
            FullName = invalidName!
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FullName)
              .WithErrorMessage("Professor faculty identity full name cannot be empty or whitespace.");
    }

    /// <summary>
    /// Assures that providing a professor display name that expands beyond 150 characters violates data constraints.
    /// </summary>
    [Fact]
    public async Task Validate_GivenFullNameExceedingLimit_ShouldHaveValidationErrorWithPreciseMessage()
    {
        // Arrange
        var bloatedName = new string('A', 151);
        var command = new RegisterProfessorCommand
        {
            FullName = bloatedName
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FullName)
              .WithErrorMessage("Full name description details cannot exceed 150 characters.");
    }

    /// <summary>
    /// Assures that if an incoming request provides an empty, null, or whitespace-only 
    /// Academic Department string, a precise validation error is logged.
    /// Note: Parameter defined as string? to cleanly eliminate analyzer type warnings.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   \t   ")]
    [InlineData(null)]
    public async Task Validate_GivenInvalidAcademicDepartment_ShouldHaveValidationErrorWithPreciseMessage(string? invalidDepartment)
    {
        // Arrange
        var command = new RegisterProfessorCommand
        {
            AcademicDepartment = invalidDepartment!
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AcademicDepartment)
              .WithErrorMessage("Academic department assignment details cannot be empty or whitespace.");
    }

    /// <summary>
    /// Assures that providing an academic department name that exceeds 100 characters violates data constraints.
    /// </summary>
    [Fact]
    public async Task Validate_GivenAcademicDepartmentExceedingLimit_ShouldHaveValidationErrorWithPreciseMessage()
    {
        // Arrange
        var bloatedDept = new string('D', 101);
        var command = new RegisterProfessorCommand
        {
            AcademicDepartment = bloatedDept
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AcademicDepartment)
              .WithErrorMessage("Academic department cannot exceed 100 characters.");
    }

    /// <summary>
    /// Assures that if an incoming command features a missing, null, or whitespace institutional rank descriptor,
    /// a validation failure is successfully recorded against the member property.
    /// Note: Parameter defined as string? to cleanly eliminate analyzer type warnings.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("     ")]
    [InlineData(null)]
    public async Task Validate_GivenInvalidRank_ShouldHaveValidationErrorWithPreciseMessage(string? invalidRank)
    {
        // Arrange
        var command = new RegisterProfessorCommand
        {
            Rank = invalidRank!
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Rank)
              .WithErrorMessage("Faculty positional rank status details cannot be empty or whitespace.");
    }

    /// <summary>
    /// Assures that providing a faculty rank string that exceeds 50 characters violates data constraints.
    /// </summary>
    [Fact]
    public async Task Validate_GivenRankExceedingLimit_ShouldHaveValidationErrorWithPreciseMessage()
    {
        // Arrange
        var bloatedRank = new string('R', 51);
        var command = new RegisterProfessorCommand
        {
            Rank = bloatedRank
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Rank)
              .WithErrorMessage("Faculty positional rank status description cannot exceed 50 characters.");
    }

    /// <summary>
    /// Assures that when the requested maximum student supervision capacity count drops below system threshold bounds
    /// (such as exactly zero or negative spaces), a validation error is successfully recorded.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-25)]
    public async Task Validate_GivenInvalidSupervisionCapacity_ShouldHaveValidationErrorWithPreciseMessage(int invalidCapacity)
    {
        // Arrange
        var command = new RegisterProfessorCommand
        {
            MaxSupervisionCapacity = invalidCapacity
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaxSupervisionCapacity)
              .WithErrorMessage("Initial maximum supervisor project capacity limit bounds must exceed zero.");
    }

    /// <summary>
    /// Assures that invalid inputs crossing baseline identity credential properties (such as malformed email syntax
    /// or simple passwords) are successfully intercepted by cross-cutting extension rules.
    /// </summary>
    [Fact]
    public async Task Validate_GivenMalformedIdentityFields_ShouldFailIdentityDryRules()
    {
        // Arrange
        var command = new RegisterProfessorCommand
        {
            Email = "invalid-email-address",
            Username = "", // Empty name values fail baseline user tracking specifications
            Password = "abc"
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Username);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
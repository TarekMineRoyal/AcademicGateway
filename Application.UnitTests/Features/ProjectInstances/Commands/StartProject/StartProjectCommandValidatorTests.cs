using AcademicGateway.Application.Features.ProjectInstances.Commands.StartProject;
using FluentValidation.TestHelper;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.ProjectInstances.Commands.StartProject;

/// <summary>
/// Production-grade validation suite ensuring strict compliance, domain invariants, and structural 
/// boundary constraints for the <see cref="StartProjectCommandValidator"/> layer.
/// </summary>
public class StartProjectCommandValidatorTests
{
    private readonly StartProjectCommandValidator _validator;

    /// <summary>
    /// Initializes a new instance of the validation suite under test.
    /// </summary>
    public StartProjectCommandValidatorTests()
    {
        _validator = new StartProjectCommandValidator();
    }

    /// <summary>
    /// Verifies that when a command provides completely valid parameters without a supervisor,
    /// validation passes cleanly with zero diagnostic errors.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithValidParametersWithoutProfessor_ShouldPassValidation()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new StartProjectCommand
        {
            TemplateId = Guid.NewGuid(),
            StudentId = Guid.NewGuid(),
            RequestedProfessorId = null
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Verifies that when a command provides completely valid parameters including a valid supervisor,
    /// validation passes cleanly with zero diagnostic errors.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithValidParametersIncludingProfessor_ShouldPassValidation()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new StartProjectCommand
        {
            TemplateId = Guid.NewGuid(),
            StudentId = Guid.NewGuid(),
            RequestedProfessorId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Enforces the constraint that an empty project template blueprint reference ID triggers an immediate validation failure.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenTemplateIdIsEmpty_ShouldFailWithRequiredMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new StartProjectCommand
        {
            TemplateId = Guid.Empty,
            StudentId = Guid.NewGuid(),
            RequestedProfessorId = null
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TemplateId)
              .WithErrorMessage("Source Project Template ID is required.");
    }

    /// <summary>
    /// Enforces the constraint that an empty student profile identifier triggers an immediate validation failure.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenStudentIdIsEmpty_ShouldFailWithRequiredMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new StartProjectCommand
        {
            TemplateId = Guid.NewGuid(),
            StudentId = Guid.Empty,
            RequestedProfessorId = null
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StudentId)
              .WithErrorMessage("Student ID is required.");
    }

    /// <summary>
    /// Asserts that when an academic supervisor is explicitly specified but contains an empty Guid value,
    /// the conditional validation framework catches and isolates it with the explicit rule error message.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenRequestedProfessorIdIsEmptyGuid_ShouldFailWithInvalidGuidMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new StartProjectCommand
        {
            TemplateId = Guid.NewGuid(),
            StudentId = Guid.NewGuid(),
            RequestedProfessorId = Guid.Empty
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RequestedProfessorId)
              .WithErrorMessage("The requested academic supervisor ID cannot be an empty Guid.");
    }

    /// <summary>
    /// Theoretical boundary verification confirming message constraints mapping inside matrix setups.
    /// Standard warning mitigation constraint applied via nullable string definition variables.
    /// </summary>
    [Theory]
    [InlineData("Source Project Template ID is required.")]
    public async Task ValidateAsync_VerifyStructuralErrorMessageDiagnostics_MitigatingAnalyzerWarnings(string? expectedMessage)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new StartProjectCommand
        {
            TemplateId = Guid.Empty,
            StudentId = Guid.NewGuid(),
            RequestedProfessorId = null
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TemplateId)
              .WithErrorMessage(expectedMessage!);
    }
}
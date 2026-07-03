using AcademicGateway.Application.Features.ProjectInstances.Commands.TransitionToSolo;
using FluentValidation.TestHelper;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.ProjectInstances.Commands.TransitionToSolo;

/// <summary>
/// Production-grade validation suite ensuring strict formatting compliance and structural 
/// boundary constraints for the <see cref="TransitionToSoloCommandValidator"/> layer.
/// </summary>
public class TransitionToSoloCommandValidatorTests
{
    private readonly TransitionToSoloCommandValidator _validator;

    /// <summary>
    /// Initializes a pristine instance of the command validator under test.
    /// </summary>
    public TransitionToSoloCommandValidatorTests()
    {
        _validator = new TransitionToSoloCommandValidator();
    }

    /// <summary>
    /// Verifies that when a command supplies a populated, non-empty ProjectInstanceId tracker token,
    /// validation completes successfully without reporting any rule violations.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithValidCommandParameters_ShouldPassValidation()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new TransitionToSoloCommand
        {
            ProjectInstanceId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Enforces the requirement that an empty or default Guid tracking code triggers an immediate structural rule failure.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenProjectInstanceIdIsEmpty_ShouldFailWithRequiredMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new TransitionToSoloCommand
        {
            ProjectInstanceId = Guid.Empty
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProjectInstanceId)
              .WithErrorMessage("Project Instance ID is required to identify the target workspace.");
    }

    /// <summary>
    /// Theoretical boundary verification confirming message constraints map accurately inside matrix structures.
    /// Standard warning mitigation constraint applied via nullable string definition variables.
    /// </summary>
    [Theory]
    [InlineData("Project Instance ID is required to identify the target workspace.")]
    public async Task ValidateAsync_VerifyStructuralErrorMessageDiagnostics_MitigatingAnalyzerWarnings(string? expectedMessage)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new TransitionToSoloCommand
        {
            ProjectInstanceId = Guid.Empty
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProjectInstanceId)
              .WithErrorMessage(expectedMessage!);
    }
}
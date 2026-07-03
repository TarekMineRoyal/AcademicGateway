using AcademicGateway.Application.Features.ProjectInstances.Commands.ConcludeProject;
using FluentValidation.TestHelper;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.ProjectInstances.Commands.ConcludeProject;

/// <summary>
/// Production-grade validation suite ensuring strict formatting constraints and domain invariant 
/// compliance for the <see cref="ConcludeProjectCommandValidator"/> layer.
/// </summary>
public class ConcludeProjectCommandValidatorTests
{
    private readonly ConcludeProjectCommandValidator _validator;

    /// <summary>
    /// Initializes a pristine instance of the validator under test.
    /// </summary>
    public ConcludeProjectCommandValidatorTests()
    {
        _validator = new ConcludeProjectCommandValidator();
    }

    /// <summary>
    /// Verifies that when a command supplies a populated, non-empty ProjectInstanceId identifier,
    /// validation passes cleanly without reporting any diagnostic errors.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithValidCommandParameters_ShouldPassValidation()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new ConcludeProjectCommand
        {
            ProjectInstanceId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Enforces the requirement that an empty or default Guid tracking code triggers an immediate rule failure.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenProjectInstanceIdIsEmpty_ShouldFailWithRequiredMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new ConcludeProjectCommand
        {
            ProjectInstanceId = Guid.Empty
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProjectInstanceId)
              .WithErrorMessage("Project Instance ID is required to conclude the workspace.");
    }

    /// <summary>
    /// Theoretical boundary verification confirming message constraints map correctly inside matrix structures.
    /// Standard warning mitigation constraint applied via nullable string definition variables.
    /// </summary>
    [Theory]
    [InlineData("Project Instance ID is required to conclude the workspace.")]
    public async Task ValidateAsync_VerifyStructuralErrorMessageDiagnostics_MitigatingAnalyzerWarnings(string? expectedMessage)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new ConcludeProjectCommand
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
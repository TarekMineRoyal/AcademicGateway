using AcademicGateway.Application.Features.ProjectInstances.Commands.CancelProject;
using FluentValidation.TestHelper;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.ProjectInstances.Commands.CancelProject;

/// <summary>
/// Production-grade validation suite ensuring formatting constraints, business logic rules,
/// and metadata boundaries are strictly enforced for the <see cref="CancelProjectCommandValidator"/>.
/// </summary>
public class CancelProjectCommandValidatorTests
{
    private readonly CancelProjectCommandValidator _validator;

    /// <summary>
    /// Initializes structural parameter rule configurations for the validation suite under test.
    /// </summary>
    public CancelProjectCommandValidatorTests()
    {
        _validator = new CancelProjectCommandValidator();
    }

    /// <summary>
    /// Verifies that when a command satisfies all structural rules with a fully populated reason text,
    /// validation passes cleanly without reporting any diagnostic errors.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithValidCommandParameters_ShouldPassValidation()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new CancelProjectCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            Reason = "The client sponsor had to relocate operational resource scopes, canceling external student work paths."
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Enforces the requirement that an empty or default ProjectInstanceId identifier triggers an immediate validation error.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenProjectInstanceIdIsEmpty_ShouldFailWithRequiredMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new CancelProjectCommand
        {
            ProjectInstanceId = Guid.Empty,
            Reason = "Valid cancellation reason context string."
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProjectInstanceId)
              .WithErrorMessage("Project Instance ID is required to cancel the workspace.");
    }

    /// <summary>
    /// Evaluates conditional logic branches proving that omitted, empty, or unpopulated cancellation reasons
    /// are completely skipped by structural constraint validators.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ValidateAsync_WhenReasonIsNullOrEmpty_ShouldPassValidation(string? missingReason)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new CancelProjectCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            Reason = missingReason
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Reason);
    }

    /// <summary>
    /// Asserts that explanatory cancellation narrative parameters cannot violate data structures by exceeding the 500 character rule limit.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenReasonExceedsMaximumLimit_ShouldFailWithLengthExceededMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var excessiveReasonText = new string('A', 501); // 501 characters - violates MaximumLength(500)

        var command = new CancelProjectCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            Reason = excessiveReasonText
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason)
              .WithErrorMessage("The cancellation reason description text cannot exceed 500 characters.");
    }

    /// <summary>
    /// Verifies precise boundary compliance behavior exactly at the upper threshold parameters of the character limit rules.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenReasonIsExactlyAtMaximumBoundary_ShouldPassValidation()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var boundaryReasonText = new string('B', 500); // Exactly 500 characters - matching upper limit boundary

        var command = new CancelProjectCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            Reason = boundaryReasonText
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Reason);
    }
}
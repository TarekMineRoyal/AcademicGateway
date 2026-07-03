using AcademicGateway.Application.Features.SupervisionRequests.Commands.ReviewSupervisionRequest;
using FluentValidation.TestHelper;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.SupervisionRequests.Commands.ReviewSupervisionRequest;

/// <summary>
/// Production-grade validation suite ensuring strict compliance, conditional branch logic rules,
/// and structural boundary constraints for the <see cref="ReviewSupervisionRequestCommandValidator"/> layer.
/// </summary>
public class ReviewSupervisionRequestCommandValidatorTests
{
    private readonly ReviewSupervisionRequestCommandValidator _validator;

    /// <summary>
    /// Initializes functional format constraints for the validation unit suite.
    /// </summary>
    public ReviewSupervisionRequestCommandValidatorTests()
    {
        _validator = new ReviewSupervisionRequestCommandValidator();
    }

    /// <summary>
    /// Verifies that when a professor accepts the matchmaking invite and omits any rejection narratives,
    /// validation passes cleanly with zero errors.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenAcceptIsTrueAndRejectionReasonIsEmpty_ShouldPassValidation()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new ReviewSupervisionRequestCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            SupervisionRequestId = Guid.NewGuid(),
            Accept = true,
            RejectionReason = null
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Verifies that when a professor declines the matchmaking invite and provides a valid explanatory 
    /// feedback string, validation passes cleanly with zero errors.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenAcceptIsFalseAndRejectionReasonIsValid_ShouldPassValidation()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new ReviewSupervisionRequestCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            SupervisionRequestId = Guid.NewGuid(),
            Accept = false,
            RejectionReason = "I lack available supervision bandwidth for additional aggregate root project tracing reviews this term."
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Enforces the constraint that an empty or default Guid tracking instance identifier triggers an immediate error.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenProjectInstanceIdIsEmpty_ShouldFailWithRequiredMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new ReviewSupervisionRequestCommand
        {
            ProjectInstanceId = Guid.Empty,
            SupervisionRequestId = Guid.NewGuid(),
            Accept = true,
            RejectionReason = null
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProjectInstanceId)
              .WithErrorMessage("Project Instance ID is required to locate the active workspace.");
    }

    /// <summary>
    /// Enforces the constraint that an empty or default Guid supervision request identifier triggers an immediate error.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenSupervisionRequestIdIsEmpty_ShouldFailWithRequiredMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new ReviewSupervisionRequestCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            SupervisionRequestId = Guid.Empty,
            Accept = true,
            RejectionReason = null
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SupervisionRequestId)
              .WithErrorMessage("Supervision Request ID is required to update the matchmaking records.");
    }

    /// <summary>
    /// Evaluates conditional logic branches proving that providing text feedback during an approval
    /// event violates core business rules and triggers the conflict message.
    /// String parameter fields explicitly typed as nullable to cleanly clear analyzer diagnostics.
    /// </summary>
    [Theory]
    [InlineData("Valid text note")]
    public async Task ValidateAsync_WhenAcceptIsTrueButRejectionReasonIsPopulated_ShouldFailWithConflictMessage(string? invalidReasonText)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new ReviewSupervisionRequestCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            SupervisionRequestId = Guid.NewGuid(),
            Accept = true,
            RejectionReason = invalidReasonText!
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RejectionReason)
              .WithErrorMessage("A rejection reason must not be provided when accepting an academic supervision request.");
    }

    /// <summary>
    /// Asserts that rejection reason parameters cannot violate data structures by exceeding the 500 character rule limit.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenAcceptIsFalseAndReasonExceedsMaximumLimit_ShouldFailWithLengthExceededMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var excessiveReasonText = new string('X', 501); // 501 characters - violates MaximumLength(500)

        var command = new ReviewSupervisionRequestCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            SupervisionRequestId = Guid.NewGuid(),
            Accept = false,
            RejectionReason = excessiveReasonText
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RejectionReason)
              .WithErrorMessage("Rejection reason feedback text cannot exceed 500 characters.");
    }

    /// <summary>
    /// Verifies precise boundary compliance behavior exactly at the upper threshold parameters of the character limit rules.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenAcceptIsFalseAndReasonIsExactlyAtMaximumBoundary_ShouldPassValidation()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var boundaryReasonText = new string('Y', 500); // Exactly 500 characters - matching upper limit boundary

        var command = new ReviewSupervisionRequestCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            SupervisionRequestId = Guid.NewGuid(),
            Accept = false,
            RejectionReason = boundaryReasonText
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RejectionReason);
    }
}
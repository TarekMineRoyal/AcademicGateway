using AcademicGateway.Application.Features.ProviderApplications.Commands.ReviewProviderApplication;
using FluentValidation.TestHelper;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.ProviderApplications.Commands.ReviewProviderApplication;

/// <summary>
/// Production-grade validation suite ensuring structural compliance, conditional branching rules,
/// and metadata boundary constraints for the <see cref="ReviewProviderApplicationCommandValidator"/> layer.
/// </summary>
public class ReviewProviderApplicationCommandValidatorTests
{
    private readonly ReviewProviderApplicationCommandValidator _validator;

    /// <summary>
    /// Initializes functional format constraints for the validation unit suite under test.
    /// </summary>
    public ReviewProviderApplicationCommandValidatorTests()
    {
        _validator = new ReviewProviderApplicationCommandValidator();
    }

    /// <summary>
    /// Verifies that when a command requests an application approval status and omits rejection tracking feedback,
    /// validation completes successfully with zero diagnostic anomalies.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenIsApprovedIsTrueAndRejectionReasonIsEmpty_ShouldPassValidation()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new ReviewProviderApplicationCommand
        {
            ApplicationId = Guid.NewGuid(),
            ReviewerId = Guid.NewGuid(),
            IsApproved = true,
            RejectionReason = null
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Verifies that when a command declines an enrollment application and attaches a valid, detailed feedback explanation statement,
    /// validation completes successfully with zero diagnostic anomalies.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenIsApprovedIsFalseAndRejectionReasonIsValid_ShouldPassValidation()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new ReviewProviderApplicationCommand
        {
            ApplicationId = Guid.NewGuid(),
            ReviewerId = Guid.NewGuid(),
            IsApproved = false,
            RejectionReason = "The corporate verification documents provided lack structural validation seal stamps from local trade offices."
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Enforces the constraint that an empty or default Guid application reference tracking key triggers an immediate failure.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenApplicationIdIsEmpty_ShouldFailWithRequiredMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new ReviewProviderApplicationCommand
        {
            ApplicationId = Guid.Empty,
            ReviewerId = Guid.NewGuid(),
            IsApproved = true,
            RejectionReason = null
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ApplicationId)
              .WithErrorMessage("Application ID is required.");
    }

    /// <summary>
    /// Enforces the constraint that an empty or default Guid auditor identity tracking key triggers an immediate failure.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenReviewerIdIsEmpty_ShouldFailWithRequiredMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new ReviewProviderApplicationCommand
        {
            ApplicationId = Guid.NewGuid(),
            ReviewerId = Guid.Empty,
            IsApproved = true,
            RejectionReason = null
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ReviewerId)
              .WithErrorMessage("Reviewer ID is required.");
    }

    /// <summary>
    /// Evaluates conditional logic branches proving that omitting text reasons during a rejection operation
    /// violates baseline workflow requirements and triggers the core requirement error message.
    /// String parameters typed explicitly as nullable reference vectors to clean analyzer outputs.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateAsync_WhenIsApprovedIsFalseAndReasonIsMissingOrBlank_ShouldFailWithRequiredMessage(string? invalidReasonText)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new ReviewProviderApplicationCommand
        {
            ApplicationId = Guid.NewGuid(),
            ReviewerId = Guid.NewGuid(),
            IsApproved = false,
            RejectionReason = invalidReasonText!
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RejectionReason)
              .WithErrorMessage("A rejection reason must be provided when declining a corporate provider application.");
    }

    /// <summary>
    /// Asserts that short explanatory descriptions falling below the minimal threshold trigger a length constraint failure.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenIsApprovedIsFalseAndReasonIsTooShort_ShouldFailWithLengthConstraintMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new ReviewProviderApplicationCommand
        {
            ApplicationId = Guid.NewGuid(),
            ReviewerId = Guid.NewGuid(),
            IsApproved = false,
            RejectionReason = "ShortText" // 10 characters required, 9 provided - triggers failure
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RejectionReason)
              .WithErrorMessage("The rejection reason must be at least 10 characters long to provide actionable feedback to the partner.");
    }

    /// <summary>
    /// Asserts that excessive explanatory parameters cannot violate storage boundaries by exceeding the 1000 character rule limit.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenIsApprovedIsFalseAndReasonExceedsMaximumLimit_ShouldFailWithLengthExceededMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var excessiveReasonText = new string('Z', 1001); // 1001 characters - violates MaximumLength(1000)

        var command = new ReviewProviderApplicationCommand
        {
            ApplicationId = Guid.NewGuid(),
            ReviewerId = Guid.NewGuid(),
            IsApproved = false,
            RejectionReason = excessiveReasonText
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RejectionReason)
              .WithErrorMessage("The rejection reason cannot exceed 1000 characters.");
    }

    /// <summary>
    /// Verifies precise boundary compliance behavior exactly at the lower and upper threshold parameters of the rules.
    /// </summary>
    [Theory]
    [InlineData(10)]
    [InlineData(1000)]
    public async Task ValidateAsync_WhenIsApprovedIsFalseAndReasonIsExactlyAtBoundaries_ShouldPassValidation(int boundaryLength)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var boundaryText = new string('B', boundaryLength);

        var command = new ReviewProviderApplicationCommand
        {
            ApplicationId = Guid.NewGuid(),
            ReviewerId = Guid.NewGuid(),
            IsApproved = false,
            RejectionReason = boundaryText
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RejectionReason);
    }

    /// <summary>
    /// Confirms conditional bypass behavior showing that when a partner profile is marked as approved,
    /// text rejection reason rules are completely suspended.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenIsApprovedIsTrue_ShouldIgnoreAnyRejectionReasonAnomalies()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        // This input would normally fail the short length rule (9 chars) if IsApproved were false,
        // but passes here as it's skipped.
        var command = new ReviewProviderApplicationCommand
        {
            ApplicationId = Guid.NewGuid(),
            ReviewerId = Guid.NewGuid(),
            IsApproved = true,
            RejectionReason = "Abc"
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
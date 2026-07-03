using AcademicGateway.Application.Features.TechSupportProposals.Commands.ReviewTechSupportProposal;
using FluentValidation.TestHelper;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.TechSupportProposals.Commands.ReviewTechSupportProposal;

/// <summary>
/// Production-grade validation suite ensuring strict compliance, conditional branching rules,
/// and structural boundary constraints for the <see cref="ReviewTechSupportProposalCommandValidator"/> layer.
/// </summary>
public class ReviewTechSupportProposalCommandValidatorTests
{
    private readonly ReviewTechSupportProposalCommandValidator _validator;

    /// <summary>
    /// Initializes functional format constraints for the validation unit suite under test.
    /// </summary>
    public ReviewTechSupportProposalCommandValidatorTests()
    {
        _validator = new ReviewTechSupportProposalCommandValidator();
    }

    /// <summary>
    /// Verifies that when an engineer proposal is accepted and any accompanying rejection reasons are omitted,
    /// validation finishes successfully with zero reported errors.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenAcceptIsTrueAndRejectionReasonIsEmpty_ShouldPassValidation()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new ReviewTechSupportProposalCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            TechSupportProposalId = Guid.NewGuid(),
            Accept = true,
            RejectionReason = null
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Verifies that when an engineer proposal is declined and a valid descriptive justification phrase is provided,
    /// validation finishes successfully with zero reported errors.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenAcceptIsFalseAndRejectionReasonIsValid_ShouldPassValidation()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new ReviewTechSupportProposalCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            TechSupportProposalId = Guid.NewGuid(),
            Accept = false,
            RejectionReason = "The architecture requirements for this workspace demand specialized virtualization tracks outside our scope."
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Enforces the requirement that an empty or default Guid tracking instance identifier triggers an immediate error.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenProjectInstanceIdIsEmpty_ShouldFailWithRequiredMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new ReviewTechSupportProposalCommand
        {
            ProjectInstanceId = Guid.Empty,
            TechSupportProposalId = Guid.NewGuid(),
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
    /// Enforces the requirement that an empty or default Guid technical support proposal identifier triggers an immediate error.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenTechSupportProposalIdIsEmpty_ShouldFailWithRequiredMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new ReviewTechSupportProposalCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            TechSupportProposalId = Guid.Empty,
            Accept = true,
            RejectionReason = null
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TechSupportProposalId)
              .WithErrorMessage("Tech Support Proposal ID is required to update the matchmaking records.");
    }

    /// <summary>
    /// Evaluates conditional logic branches proving that providing text feedback during an approval event
    /// violates core validation logic rules and triggers a conflict error message.
    /// String parameter fields explicitly typed as nullable to cleanly clear static analyzer warnings.
    /// </summary>
    [Theory]
    [InlineData("Declining notes text")]
    [InlineData("   ")]
    public async Task ValidateAsync_WhenAcceptIsTrueButRejectionReasonIsPopulated_ShouldFailWithConflictMessage(string? invalidReasonText)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new ReviewTechSupportProposalCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            TechSupportProposalId = Guid.NewGuid(),
            Accept = true,
            RejectionReason = invalidReasonText!
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RejectionReason)
              .WithErrorMessage("A rejection reason must not be provided when accepting a technical support proposal.");
    }

    /// <summary>
    /// Asserts that rejection reason parameters cannot violate data structures by exceeding the 500 character rule limit.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenAcceptIsFalseAndReasonExceedsMaximumLimit_ShouldFailWithLengthExceededMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var excessiveReasonText = new string('Z', 501); // 501 characters - violates MaximumLength(500)

        var command = new ReviewTechSupportProposalCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            TechSupportProposalId = Guid.NewGuid(),
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
        var boundaryReasonText = new string('W', 500); // Exactly 500 characters - matching upper limit boundary

        var command = new ReviewTechSupportProposalCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            TechSupportProposalId = Guid.NewGuid(),
            Accept = false,
            RejectionReason = boundaryReasonText
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RejectionReason);
    }
}
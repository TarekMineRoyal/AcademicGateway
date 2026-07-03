using AcademicGateway.Application.Features.TechSupportProposals.Commands.ProposeTechSupport;
using FluentValidation.TestHelper;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.TechSupportProposals.Commands.ProposeTechSupport;

/// <summary>
/// Production-grade validation suite ensuring strict structural constraints, conditional rules execution,
/// and domain invariants compliance for the <see cref="ProposeTechSupportCommandValidator"/> layer.
/// </summary>
public class ProposeTechSupportCommandValidatorTests
{
    private readonly ProposeTechSupportCommandValidator _validator;

    /// <summary>
    /// Initializes functional format constraints for the validation unit suite.
    /// </summary>
    public ProposeTechSupportCommandValidatorTests()
    {
        _validator = new ProposeTechSupportCommandValidator();
    }

    /// <summary>
    /// Verifies that when a command supplies a populated project instance tracking ID, a valid engineer account tracking ID,
    /// and a fully compliant introductory message, validation passes successfully with zero errors.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithValidCommandParameters_ShouldPassValidation()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new ProposeTechSupportCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            TechSupportAccountId = Guid.NewGuid(),
            Message = "Senior DevOps architect with extensive experience maintaining platform runtime invariants."
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Enforces the constraint that an empty or default Guid project instance workspace tracking ID triggers an immediate structural rule failure.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenProjectInstanceIdIsEmpty_ShouldFailWithRequiredMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new ProposeTechSupportCommand
        {
            ProjectInstanceId = Guid.Empty,
            TechSupportAccountId = Guid.NewGuid(),
            Message = "Valid introductory message statement text."
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProjectInstanceId)
              .WithErrorMessage("Project Instance ID is required to route the technical support proposal.");
    }

    /// <summary>
    /// Enforces the constraint that an empty or default Guid technical support account tracking identifier triggers an immediate structural rule failure.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenTechSupportAccountIdIsEmpty_ShouldFailWithRequiredMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new ProposeTechSupportCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            TechSupportAccountId = Guid.Empty,
            Message = "Valid introductory message statement text."
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TechSupportAccountId)
              .WithErrorMessage("Tech Support Account ID is required to identify the proposed engineer.");
    }

    /// <summary>
    /// Evaluates conditional logic branches proving that omitted, empty, or completely unpopulated 
    /// messages are safely skipped by structural constraint rules.
    /// String arguments typed as nullable to cleanly satisfy code analyzer metrics.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ValidateAsync_WhenMessageIsNullOrEmpty_ShouldPassValidation(string? missingMessage)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new ProposeTechSupportCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            TechSupportAccountId = Guid.NewGuid(),
            Message = missingMessage!
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Message);
    }

    /// <summary>
    /// Asserts that commentary narrative parameters cannot violate system storage boundaries by exceeding the 1000 character rule limit.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenMessageExceedsMaximumLimit_ShouldFailWithLengthExceededMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var excessiveMessageText = new string('A', 1001); // 1001 characters - violates MaximumLength(1000)

        var command = new ProposeTechSupportCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            TechSupportAccountId = Guid.NewGuid(),
            Message = excessiveMessageText
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Message)
              .WithErrorMessage("The introductory message text cannot exceed 1000 characters.");
    }

    /// <summary>
    /// Verifies precise boundary compliance behavior exactly at the upper limit capacity marker threshold.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenMessageIsExactlyAtMaximumBoundary_ShouldPassValidation()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var boundaryMessageText = new string('B', 1000); // Exactly 1000 characters - matching upper limit boundary

        var command = new ProposeTechSupportCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            TechSupportAccountId = Guid.NewGuid(),
            Message = boundaryMessageText
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Message);
    }
}
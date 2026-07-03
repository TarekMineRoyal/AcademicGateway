using AcademicGateway.Application.Features.SupervisionRequests.Commands.SubmitSupervisionRequest;
using FluentValidation.TestHelper;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.SupervisionRequests.Commands.SubmitSupervisionRequest;

/// <summary>
/// Production-grade validation suite ensuring structural compliance, text boundaries, 
/// and core domain invariant checks for the <see cref="SubmitSupervisionRequestCommandValidator"/> layer.
/// </summary>
public class SubmitSupervisionRequestCommandValidatorTests
{
    private readonly SubmitSupervisionRequestCommandValidator _validator;

    /// <summary>
    /// Initializes a pristine instance of the validator under test.
    /// </summary>
    public SubmitSupervisionRequestCommandValidatorTests()
    {
        _validator = new SubmitSupervisionRequestCommandValidator();
    }

    /// <summary>
    /// Verifies that when a command satisfies all structural rules with valid identifiers and a 
    /// completely compliant pitch text, validation passes successfully with zero errors.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithValidCommandParameters_ShouldPassValidation()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new SubmitSupervisionRequestCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            ProfessorId = Guid.NewGuid(),
            PitchText = "I am deeply eager to explore advanced distributed tracing strategies under your expert guidance."
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Enforces the constraint that an empty or default Guid workspace project instance identifier 
    /// triggers an immediate validation rules check error.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenProjectInstanceIdIsEmpty_ShouldFailWithRequiredMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new SubmitSupervisionRequestCommand
        {
            ProjectInstanceId = Guid.Empty,
            ProfessorId = Guid.NewGuid(),
            PitchText = "Valid motivation text exceeding the baseline twenty character threshold boundary."
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProjectInstanceId)
              .WithErrorMessage("Project Instance ID is required to associate the matchmaking request.");
    }

    /// <summary>
    /// Enforces the requirement that a blank or unassigned target faculty identifier yields an explicit validation diagnostic.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenProfessorIdIsEmpty_ShouldFailWithRequiredMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new SubmitSupervisionRequestCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            ProfessorId = Guid.Empty,
            PitchText = "Valid motivation text exceeding the baseline twenty character threshold boundary."
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProfessorId)
              .WithErrorMessage("Target Professor ID is required to route the invitation.");
    }

    /// <summary>
    /// Evaluates missing text parameters ensuring nulls, blanks, or space arrays trigger the core missing pitch error.
    /// String parameters typed explicitly as nullable reference values to cleanly mitigate compiler diagnostics.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateAsync_WhenPitchTextIsMissingOrBlank_ShouldFailWithRequiredMessage(string? invalidPitchText)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new SubmitSupervisionRequestCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            ProfessorId = Guid.NewGuid(),
            PitchText = invalidPitchText!
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PitchText)
              .WithErrorMessage("A motivation pitch statement is required.");
    }

    /// <summary>
    /// Asserts that a motivation text scaling below twenty characters falls short of baseline requirements and triggers a warning.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenPitchTextIsTooShort_ShouldFailWithLengthConstraintMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new SubmitSupervisionRequestCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            ProfessorId = Guid.NewGuid(),
            PitchText = "Too short pitch" // 15 characters - violates MinimumLength(20)
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PitchText)
              .WithErrorMessage("Your pitch statement must provide at least 20 characters of contextual detail.");
    }

    /// <summary>
    /// Asserts that massive text strings cannot violate system storage boundaries by exceeding the 1500 character rule limit.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenPitchTextExceedsMaximumLimit_ShouldFailWithLengthExceededMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var excessivePitchText = new string('A', 1501); // 1501 characters - violates MaximumLength(1500)

        var command = new SubmitSupervisionRequestCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            ProfessorId = Guid.NewGuid(),
            PitchText = excessivePitchText
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PitchText)
              .WithErrorMessage("Your pitch statement cannot exceed 1500 characters.");
    }

    /// <summary>
    /// Verifies precise boundary compliance behavior exactly at the lower and upper threshold parameters of the length rules.
    /// </summary>
    [Theory]
    [InlineData(20)]
    [InlineData(1500)]
    public async Task ValidateAsync_WhenPitchTextIsExactlyAtLengthBoundaries_ShouldPassValidation(int boundaryLength)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var boundaryText = new string('B', boundaryLength);

        var command = new SubmitSupervisionRequestCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            ProfessorId = Guid.NewGuid(),
            PitchText = boundaryText
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PitchText);
    }
}
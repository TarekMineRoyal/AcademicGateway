using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Application.Features.ProjectInstances.Commands.SetProjectEndDate;
using FluentValidation.TestHelper;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.ProjectInstances.Commands.SetProjectEndDate;

/// <summary>
/// Production-grade validation suite ensuring strict timeline compliance, clock-dependent invariants,
/// and structural data boundaries for the <see cref="SetProjectEndDateCommandValidator"/> layer.
/// </summary>
public class SetProjectEndDateCommandValidatorTests
{
    private readonly Mock<IDateTimeProvider> _mockDateTimeProvider;
    private readonly SetProjectEndDateCommandValidator _validator;
    private readonly DateTime _frozenUtcNow;

    /// <summary>
    /// Initializes functional format constraints, anchoring the system clock simulation.
    /// </summary>
    public SetProjectEndDateCommandValidatorTests()
    {
        _mockDateTimeProvider = new Mock<IDateTimeProvider>();

        // Establish a fixed, deterministic UtcNow anchor to ensure stable timeline evaluations
        _frozenUtcNow = new DateTime(2026, 7, 3, 12, 0, 0, DateTimeKind.Utc);
        _mockDateTimeProvider.Setup(x => x.UtcNow).Returns(_frozenUtcNow);

        _validator = new SetProjectEndDateCommandValidator(_mockDateTimeProvider.Object);
    }

    /// <summary>
    /// Verifies that when a command supplies a populated instance tracking ID and a deadline securely 
    /// positioned in the future, validation passes cleanly with zero reported anomalies.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithValidCommandParameters_ShouldPassValidation()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new SetProjectEndDateCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            NewEndDate = _frozenUtcNow.AddDays(14) // Safely in the future
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Enforces the constraint that an empty or default Guid tracking key triggers an immediate structural rule failure.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenProjectInstanceIdIsEmpty_ShouldFailWithRequiredMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new SetProjectEndDateCommand
        {
            ProjectInstanceId = Guid.Empty,
            NewEndDate = _frozenUtcNow.AddDays(7)
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProjectInstanceId)
              .WithErrorMessage("Project Instance ID is required to update its calendar boundaries.");
    }

    /// <summary>
    /// Asserts that when the newly proposed deadline matches the current system clock exactly, 
    /// validation fails because the business rule strictly mandates a future chronological position.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenNewEndDateIsExactlyEqualToUtcNow_ShouldFailWithFutureDateMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new SetProjectEndDateCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            NewEndDate = _frozenUtcNow // Triggers boundary check failure on GreaterThan constraint
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewEndDate)
              .WithErrorMessage("The newly proposed project end date must be set to a future calendar date.");
    }

    /// <summary>
    /// Validates that a proposed deadline historical or past-dated relative to the clock mock value fails validation.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenNewEndDateIsInThePast_ShouldFailWithFutureDateMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new SetProjectEndDateCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            NewEndDate = _frozenUtcNow.AddHours(-1) // Chronologically behind system clock
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewEndDate)
              .WithErrorMessage("The newly proposed project end date must be set to a future calendar date.");
    }

    /// <summary>
    /// Verifies precise microsecond-level boundary compliance right at the minimum forward edge threshold point.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenNewEndDateIsOneMillisecondInFuture_ShouldPassValidation()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new SetProjectEndDateCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            NewEndDate = _frozenUtcNow.AddMilliseconds(1)
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.NewEndDate);
    }

    /// <summary>
    /// Theoretical boundary verification confirming message constraints map accurately inside matrix structures.
    /// Standard warning mitigation constraint applied via nullable string definition variables.
    /// </summary>
    [Theory]
    [InlineData("The newly proposed project end date must be set to a future calendar date.")]
    public async Task ValidateAsync_VerifyStructuralErrorMessageDiagnostics_MitigatingAnalyzerWarnings(string? expectedMessage)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new SetProjectEndDateCommand
        {
            ProjectInstanceId = Guid.NewGuid(),
            NewEndDate = _frozenUtcNow.AddDays(-5)
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewEndDate)
              .WithErrorMessage(expectedMessage!);
    }
}
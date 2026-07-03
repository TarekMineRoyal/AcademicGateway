using AcademicGateway.Application.Features.TechSupportAccounts.Commands.CreateTechSupportAccount;
using FluentValidation.TestHelper;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.TechSupportAccounts.Commands.CreateTechSupportAccount;

/// <summary>
/// Production-grade validation suite ensuring formatting constraints, complex identity credential rules,
/// and metadata boundaries are strictly enforced for the <see cref="CreateTechSupportAccountCommand"/>.
/// </summary>
public class CreateTechSupportAccountCommandValidatorTests
{
    private readonly CreateTechSupportAccountCommandValidator _validator;

    /// <summary>
    /// Initializes functional format constraints for the validation unit suite.
    /// </summary>
    public CreateTechSupportAccountCommandValidatorTests()
    {
        _validator = new CreateTechSupportAccountCommandValidator();
    }

    /// <summary>
    /// Verifies that when a command satisfies all structural rules, validation passes without error flags.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithValidCommandParameters_ShouldPassValidation()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = Guid.NewGuid(),
            Email = "support.agent@academicgateway.org",
            Password = "SecurePassword123!", // Satisfies length, upper, lower, digit, and special char rules
            StaffNumber = "EMP-998822",
            SupportTier = "Tier 2 Helpdesk"
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Ensures that an empty or missing structural provider primary tracking key fails validation bounds.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenProviderIdIsEmpty_ShouldFailWithRequiredMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = Guid.Empty,
            Email = "support.agent@academicgateway.org",
            Password = "SecurePassword123!",
            StaffNumber = "EMP-998822",
            SupportTier = "Tier 2 Helpdesk"
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProviderId)
              .WithErrorMessage("Provider ID is required.");
    }

    /// <summary>
    /// Tests the email presence validation checks for missing, empty, or whitespace allocations.
    /// String parameters explicitly labeled as nullable to clean analyzer diagnostic outputs.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateAsync_WhenEmailIsMissingOrBlank_ShouldFailWithRequiredMessage(string? invalidEmail)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = Guid.NewGuid(),
            Email = invalidEmail!,
            Password = "SecurePassword123!",
            StaffNumber = "EMP-998822",
            SupportTier = "Tier 2 Helpdesk"
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email address is required and cannot be empty.");
    }

    /// <summary>
    /// Ensures that malformed structural schemas for corporate electronic email vectors trigger standard diagnostic errors.
    /// </summary>
    [Theory]
    [InlineData("plain-text-string")]
    [InlineData("missingDomain@")]
    [InlineData("@missingUser.com")]
    [InlineData("spaces in@email.org")]
    public async Task ValidateAsync_WhenEmailStructureIsMalformed_ShouldFailWithFormatErrorMessage(string? malformedEmail)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = Guid.NewGuid(),
            Email = malformedEmail!,
            Password = "SecurePassword123!",
            StaffNumber = "EMP-998822",
            SupportTier = "Tier 2 Helpdesk"
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("A legitimate, standard email address structure format is required.");
    }

    /// <summary>
    /// Asserts that email tracking strings violating scale metrics above 256 characters fail cleanly.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenEmailExceedsMaximumLimit_ShouldFailWithBoundaryErrorMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var excessiveEmail = new string('a', 245) + "@academicgateway.org"; // Length = 266 characters

        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = Guid.NewGuid(),
            Email = excessiveEmail,
            Password = "SecurePassword123!",
            StaffNumber = "EMP-998822",
            SupportTier = "Tier 2 Helpdesk"
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email address cannot exceed the database boundary scale limit of 256 characters.");
    }

    /// <summary>
    /// Checks basic missing password constraints.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateAsync_WhenPasswordIsMissingOrBlank_ShouldFailWithRequiredMessage(string? invalidPassword)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = Guid.NewGuid(),
            Email = "support.agent@academicgateway.org",
            Password = invalidPassword!,
            StaffNumber = "EMP-998822",
            SupportTier = "Tier 2 Helpdesk"
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Account security password is required.");
    }

    /// <summary>
    /// Verifies short passwords failing validation metrics below the minimal scale bounds threshold.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenPasswordIsTooShort_ShouldFailWithMinLengthErrorMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = Guid.NewGuid(),
            Email = "support.agent@academicgateway.org",
            Password = "P1!a237", // 7 characters - violates MinimumLength(8)
            StaffNumber = "EMP-998822",
            SupportTier = "Tier 2 Helpdesk"
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password complexity requires at least 8 characters.");
    }

    /// <summary>
    /// Evaluates extensive length truncation scenarios where credentials exceed standard persistence limitations.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenPasswordExceedsMaximumLimit_ShouldFailWithMaxLengthErrorMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var longPassword = new string('A', 96) + "123!"; // 100 characters acceptable, 101 violates upper bound

        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = Guid.NewGuid(),
            Email = "support.agent@academicgateway.org",
            Password = longPassword + "X", // 101 characters
            StaffNumber = "EMP-998822",
            SupportTier = "Tier 2 Helpdesk"
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password parameters cannot exceed an upper limit of 100 characters.");
    }

    /// <summary>
    /// Multi-faceted entropy assessment proving complex character patterns fail rule requirements step by step.
    /// </summary>
    [Theory]
    [InlineData("lowercase123!", "Password complexity rules require at least one uppercase letter (A-Z).")]
    [InlineData("UPPERCASE123!", "Password complexity rules require at least one lowercase letter (a-z).")]
    [InlineData("MixedCaseNoDigits!", "Password complexity rules require at least one structural numeric digit (0-9).")]
    [InlineData("MixedCaseWithDigits123", "Password complexity rules require at least one custom non-alphanumeric special character.")]
    public async Task ValidateAsync_WhenPasswordFailsEntropyRequirements_ShouldFailWithTargetedComplexityMessage(string? weakPassword, string expectedErrorMessage)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = Guid.NewGuid(),
            Email = "support.agent@academicgateway.org",
            Password = weakPassword!,
            StaffNumber = "EMP-998822",
            SupportTier = "Tier 2 Helpdesk"
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage(expectedErrorMessage);
    }

    /// <summary>
    /// Ensures that missing metadata parameters for tracking staff identities trigger validation blocks.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateAsync_WhenStaffNumberIsMissingOrBlank_ShouldFailWithRequiredMessage(string? invalidStaffNumber)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = Guid.NewGuid(),
            Email = "support.agent@academicgateway.org",
            Password = "SecurePassword123!",
            StaffNumber = invalidStaffNumber!,
            SupportTier = "Tier 2 Helpdesk"
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StaffNumber)
              .WithErrorMessage("Staff number code is required.");
    }

    /// <summary>
    /// Asserts that corporate identifier scales cannot overflow backend storage space thresholds.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenStaffNumberExceedsMaximumLimit_ShouldFailWithLengthExceededMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = Guid.NewGuid(),
            Email = "support.agent@academicgateway.org",
            Password = "SecurePassword123!",
            StaffNumber = new string('X', 51), // 51 characters - violates MaximumLength(50)
            SupportTier = "Tier 2 Helpdesk"
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StaffNumber)
              .WithErrorMessage("Staff number cannot exceed 50 characters.");
    }

    /// <summary>
    /// Confirms missing administrative tier categories trigger application layer exceptions at entry bounds.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateAsync_WhenSupportTierIsMissingOrBlank_ShouldFailWithRequiredMessage(string? invalidSupportTier)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = Guid.NewGuid(),
            Email = "support.agent@academicgateway.org",
            Password = "SecurePassword123!",
            StaffNumber = "EMP-998822",
            SupportTier = invalidSupportTier!
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SupportTier)
              .WithErrorMessage("Support tier assignment level is required.");
    }

    /// <summary>
    /// Evaluates upper-bound tier text allocation dimensions to lock operational safety matrices.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WhenSupportTierExceedsMaximumLimit_ShouldFailWithLengthExceededMessage()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = Guid.NewGuid(),
            Email = "support.agent@academicgateway.org",
            Password = "SecurePassword123!",
            StaffNumber = "EMP-998822",
            SupportTier = new string('Z', 51) // 51 characters - violates MaximumLength(50)
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SupportTier)
              .WithErrorMessage("Support tier cannot exceed 50 characters.");
    }

    /// <summary>
    /// Asserts exact edge-case alignment mapping thresholds right at the maximal limit markers.
    /// </summary>
    [Theory]
    [InlineData(50)]
    public async Task ValidateAsync_WhenFieldsAreExactlyAtMaximumBoundaries_ShouldPassValidation(int maximumValidLength)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new CreateTechSupportAccountCommand
        {
            ProviderId = Guid.NewGuid(),
            Email = "support.agent@academicgateway.org",
            Password = "SecurePassword123!",
            StaffNumber = new string('M', maximumValidLength),
            SupportTier = new string('N', maximumValidLength)
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.StaffNumber);
        result.ShouldNotHaveValidationErrorFor(x => x.SupportTier);
    }
}
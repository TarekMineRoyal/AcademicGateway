using AcademicGateway.Application.Features.Providers.Commands.RegisterProvider;
using FluentValidation.TestHelper;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Providers.Commands.RegisterProvider;

/// <summary>
/// Contains isolated unit tests for the <see cref="RegisterProviderCommandValidator"/>.
/// Evaluates input validation logic, regex format prefixes, string limits, and error payload messaging.
/// </summary>
public class RegisterProviderCommandValidatorTests
{
    private readonly RegisterProviderCommandValidator _validator;

    /// <summary>
    /// Initializes a pristine instance of the validator under evaluation.
    /// </summary>
    public RegisterProviderCommandValidatorTests()
    {
        _validator = new RegisterProviderCommandValidator();
    }

    /// <summary>
    /// Assures that if an incoming corporate registration request provides an empty or 
    /// whitespace-only Company Name string, a precise validation error is logged.
    /// </summary>
    [Fact]
    public async Task Validate_GivenEmptyCompanyName_ShouldHaveValidationErrorWithPreciseMessage()
    {
        // Arrange
        var command = new RegisterProviderCommand
        {
            CompanyName = string.Empty
        };

        // Act
        // Best Practice (xUnit1051): Pass TestContext.Current.CancellationToken to allow responsive test run controls.
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CompanyName)
              .WithErrorMessage("Company name is required and cannot be empty.");
    }

    /// <summary>
    /// Assures that if a website URL is supplied but lacks a valid security protocol prefix 
    /// (such as http:// or https://), the custom regex rule triggers a specific formatting block error.
    /// </summary>
    [Fact]
    public async Task Validate_GivenInvalidWebsiteUrlProtocol_ShouldHaveValidationErrorWithPreciseMessage()
    {
        // Arrange
        var command = new RegisterProviderCommand
        {
            WebsiteUrl = "www.invalid-corporate-prefix.com" // Malformed input configuration lacking protocol text
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WebsiteUrl)
              .WithErrorMessage("Website URL must start with a valid http:// or https:// protocol prefix.");
    }

    /// <summary>
    /// Assures that when all necessary core identity, corporate layout details, and format 
    /// properties fulfill constraint specifications, the validator passes cleanly with no errors.
    /// </summary>
    [Fact]
    public async Task Validate_GivenValidCommandConfiguration_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        // Best Practice: Fully populate all verified fields that carry validation rules 
        // to prevent false-negative failures stemming from unrelated missing inputs.
        var command = new RegisterProviderCommand
        {
            Email = "provider@example.com",
            Username = "provideruser",
            Password = "ValidPassword123!",
            CompanyName = "Tech Corp",
            CompanyDescription = "An enterprise software engineering firm specialized in cloud-native infrastructure solutions.", // Required by system validation rules
            WebsiteUrl = "https://techcorp.com"
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
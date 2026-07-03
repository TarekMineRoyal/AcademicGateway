using AcademicGateway.Application.Features.Providers.Commands.RegisterProvider;
using FluentValidation.TestHelper;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Providers.Commands.RegisterProvider;

/// <summary>
/// Contains isolated unit tests for the <see cref="RegisterProviderCommandValidator"/>.
/// Evaluates input validation logic, identity extensions, character bounds, 
/// conditional short-circuit blocks, and error payload messaging.
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
    /// Assures that when all necessary core identity, corporate layout details, and format 
    /// properties fulfill constraint specifications, the validator passes cleanly with no errors.
    /// </summary>
    [Fact]
    public async Task Validate_GivenValidCommandConfiguration_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var command = new RegisterProviderCommand
        {
            Email = "provider@example.com",
            Username = "provideruser",
            Password = "ValidPassword123!",
            CompanyName = "Tech Corp",
            CompanyDescription = "An enterprise software engineering firm specialized in cloud-native infrastructure solutions.",
            WebsiteUrl = "https://techcorp.com"
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Assures that if an incoming corporate registration request provides an empty, null, or 
    /// whitespace-only Company Name string, a precise validation error is logged.
    /// Note: Input marked as string? to completely avoid reference type warning diagnostics.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("     ")]
    [InlineData(null)]
    public async Task Validate_GivenInvalidCompanyName_ShouldHaveValidationErrorWithPreciseMessage(string? invalidName)
    {
        // Arrange
        var command = new RegisterProviderCommand
        {
            CompanyName = invalidName!
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CompanyName)
              .WithErrorMessage("Company name is required and cannot be empty.");
    }

    /// <summary>
    /// Assures that providing a corporate company name that exceeds 100 characters triggers a validation failure.
    /// </summary>
    [Fact]
    public async Task Validate_GivenCompanyNameExceedingLimit_ShouldHaveValidationErrorWithPreciseMessage()
    {
        // Arrange
        var bloatedName = new string('X', 101);
        var command = new RegisterProviderCommand
        {
            CompanyName = bloatedName
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CompanyName)
              .WithErrorMessage("Company name description records cannot exceed 100 characters.");
    }

    /// <summary>
    /// Assures that if an incoming command features a missing, whitespace, or null profile description,
    /// a validation failure is successfully recorded against the member.
    /// Note: Input marked as string? to completely avoid reference type warning diagnostics.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" \n \t ")]
    [InlineData(null)]
    public async Task Validate_GivenInvalidCompanyDescription_ShouldHaveValidationErrorWithPreciseMessage(string? invalidDesc)
    {
        // Arrange
        var command = new RegisterProviderCommand
        {
            CompanyDescription = invalidDesc!
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CompanyDescription)
              .WithErrorMessage("Company operational background and industry focus details are required.");
    }

    /// <summary>
    /// Assures that providing a business overview description that exceeds 2000 characters triggers a validation failure.
    /// </summary>
    [Fact]
    public async Task Validate_GivenCompanyDescriptionExceedingLimit_ShouldHaveValidationErrorWithPreciseMessage()
    {
        // Arrange
        var bloatedDescription = new string('D', 2001);
        var command = new RegisterProviderCommand
        {
            CompanyDescription = bloatedDescription
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CompanyDescription)
              .WithErrorMessage("Company descriptive profiles cannot exceed 2000 characters.");
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
            WebsiteUrl = "www.invalid-corporate-prefix.com"
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WebsiteUrl)
              .WithErrorMessage("Website URL must start with a valid http:// or https:// protocol prefix.");
    }

    /// <summary>
    /// Assures that providing a corporate website URL link that exceeds 200 characters triggers a validation failure.
    /// </summary>
    [Fact]
    public async Task Validate_GivenWebsiteUrlExceedingLimit_ShouldHaveValidationErrorWithPreciseMessage()
    {
        // Arrange
        var basePrefix = "https://";
        var bloatedUrl = basePrefix + new string('w', 201 - basePrefix.Length);

        var command = new RegisterProviderCommand
        {
            WebsiteUrl = bloatedUrl
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WebsiteUrl)
              .WithErrorMessage("Website link description URL cannot exceed 200 characters.");
    }

    /// <summary>
    /// Assures that if the website URL argument is omitted entirely or passed as an empty string,
    /// the validator's internal When short-circuit block passes it safely without executing sub-rules.
    /// Note: Input marked as string? to completely avoid reference type warning diagnostics.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_GivenNullOrEmptyWebsiteUrl_ShouldShortCircuitAndNotHaveValidationErrorsForWebsite(string? blankUrl)
    {
        // Arrange
        var command = new RegisterProviderCommand
        {
            WebsiteUrl = blankUrl
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.WebsiteUrl);
    }

    /// <summary>
    /// Assures that invalid values crossing basic identity formats (such as empty email backgrounds) 
    /// are successfully caught by dry extension intercept rule mappings.
    /// </summary>
    [Fact]
    public async Task Validate_GivenMalformedIdentityFields_ShouldFailIdentityDryRules()
    {
        // Arrange
        var command = new RegisterProviderCommand
        {
            Email = "not-an-email",
            Username = "", // Violates non-empty criteria rules
            Password = "weak"
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Username);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
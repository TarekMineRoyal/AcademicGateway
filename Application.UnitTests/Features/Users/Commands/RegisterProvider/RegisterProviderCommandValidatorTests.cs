using AcademicGateway.Application.Features.Users.Commands.RegisterProvider;
using FluentValidation.TestHelper;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Features.Users.Commands.RegisterProvider;

public class RegisterProviderCommandValidatorTests
{
    private readonly RegisterProviderCommandValidator _validator;

    public RegisterProviderCommandValidatorTests()
    {
        _validator = new RegisterProviderCommandValidator();
    }

    [Fact]
    public async Task Should_Have_Error_When_OrganizationName_Is_Empty()
    {
        // Arrange
        var command = new RegisterProviderCommand { OrganizationName = string.Empty };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OrganizationName)
              .WithErrorMessage("Organization name is required.");
    }

    [Fact]
    public async Task Should_Have_Error_When_WebsiteUrl_Is_Invalid()
    {
        // Arrange
        var command = new RegisterProviderCommand { WebsiteUrl = "www.invalid-url.com" }; // Missing http(s)://

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WebsiteUrl)
              .WithErrorMessage("Website URL must start with http:// or https://");
    }

    [Fact]
    public async Task Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new RegisterProviderCommand
        {
            Email = "provider@example.com",
            Username = "provideruser",
            Password = "ValidPassword123!",
            OrganizationName = "Tech Corp",
            Industry = "Software",
            WebsiteUrl = "https://techcorp.com"
        };

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
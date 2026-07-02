using AcademicGateway.Application.Features.Providers.Commands.RegisterProvider;
using AcademicGateway.Domain.Providers;
using FluentAssertions;
using FluentValidation;
using IntegrationTests.Infrastructure;
using Xunit;

namespace AcademicGateway.IntegrationTests.Subdomains.Providers.Commands;

/// <summary>
/// Integration tests verifying validation metrics, user provisioning side-effects, 
/// and core aggregate profile creation inside the corporate Provider registration pipeline.
/// </summary>
[Collection("SharedDatabase")]
public class RegisterProviderTests : BaseIntegrationTest
{
    public RegisterProviderTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Ensures that executing a valid registration command builds out both the identity access records
    /// and a corresponding relational <see cref="Provider"/> aggregate root defaulting to an unverified state.
    /// </summary>
    [Fact]
    public async Task Should_RegisterProviderAndCreateProfile_WhenCommandIsValid()
    {
        // --- 1. ARRANGE ---
        var command = new RegisterProviderCommand
        {
            Email = "new.provider@academicgateway.com",
            Username = "ag_provider",
            Password = "SecurePassword123!",
            CompanyName = "Innovate Academy",
            CompanyDescription = "Advanced Academic Curricula and Educational Technologies",
            WebsiteUrl = "https://innovate.academicgateway.com"
        };

        // --- 2. ACT ---
        // The modernized registration endpoint returns a strongly-typed Guid identifier
        Guid providerId = await SendAsync(command);

        // --- 3. ASSERT ---
        providerId.Should().NotBeEmpty();

        // Query the data store directly to verify aggregate mapping boundaries
        var providerProfile = await FindAsync<Provider>(providerId);
        providerProfile.Should().NotBeNull();
        providerProfile!.CompanyName.Should().Be(command.CompanyName);
        providerProfile.CompanyDescription.Should().Be(command.CompanyDescription);
        providerProfile.WebsiteUrl.Should().Be(command.WebsiteUrl);
        providerProfile.IsVerified.Should().BeFalse(); // Safety boundary: New providers must start unverified
    }

    /// <summary>
    /// Ensures that trying to pass an invalid protocol prefix through the website field
    /// is caught by text pattern validation interceptors, throwing a <see cref="ValidationException"/>.
    /// </summary>
    [Fact]
    public async Task Should_ThrowValidationException_WhenWebsiteUrlIsMalformed()
    {
        // --- 1. ARRANGE ---
        var command = new RegisterProviderCommand
        {
            Email = "invalid.url@academicgateway.com",
            Username = "badurlprovider",
            Password = "SecurePassword123!",
            CompanyName = "Innovate Academy",
            CompanyDescription = "Educational Infrastructure Partners",
            WebsiteUrl = "ftp://malicious-site.com" // Violates formatting criteria (must leverage http or https protocols)
        };

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(command);

        // --- 3. ASSERT ---
        await act.Should().ThrowAsync<ValidationException>();
    }

    /// <summary>
    /// Ensures that commands missing mandatory fields or failing password password complexity rules
    /// are short-circuited at the application gateway layer, throwing a <see cref="ValidationException"/>.
    /// </summary>
    [Fact]
    public async Task Should_ThrowValidationException_WhenRequiredFieldsAreMissing()
    {
        // --- 1. ARRANGE ---
        var command = new RegisterProviderCommand
        {
            Email = "missing.fields@academicgateway.com",
            Username = "missingfields",
            Password = "123", // Non-compliant length configuration (must satisfy minimum lengths)
            CompanyName = "", // Violates non-empty rule criteria
            CompanyDescription = "" // Violates non-empty rule criteria
        };

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(command);

        // --- 3. ASSERT ---
        await act.Should().ThrowAsync<ValidationException>();
    }
}
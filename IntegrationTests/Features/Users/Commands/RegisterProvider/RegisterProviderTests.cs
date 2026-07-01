using AcademicGateway.Application.Features.Users.Commands.RegisterProvider;
using Domain.Providers;
using FluentAssertions;
using FluentValidation;
using Xunit;

namespace AcademicGateway.IntegrationTests.Features.Users.Commands.RegisterProvider;

public class RegisterProviderTests : BaseIntegrationTest
{
    public RegisterProviderTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Should_RegisterProviderAndCreateProfile_WhenCommandIsValid()
    {
        // --- 1. ARRANGE ---
        var command = new RegisterProviderCommand
        {
            Email = "new.provider@academicgateway.com",
            Username = "ag_provider",
            Password = "SecurePassword123!",
            OrganizationName = "Innovate Academy",
            Industry = "EdTech",
            WebsiteUrl = "https://innovate.academicgateway.com"
        };

        // --- 2. ACT ---
        var userId = await SendAsync(command);

        // --- 3. ASSERT ---
        userId.Should().NotBeNullOrEmpty();

        // Query the database directly to verify the Profile entity exists
        var providerProfile = await FindAsync<Provider>(userId);
        providerProfile.Should().NotBeNull();
        providerProfile!.OrganizationName.Should().Be(command.OrganizationName);
        providerProfile.Industry.Should().Be(command.Industry);
        providerProfile.WebsiteUrl.Should().Be(command.WebsiteUrl);
        providerProfile.IsVerified.Should().BeFalse(); // New providers must start unverified
    }

    [Fact]
    public async Task Should_ThrowValidationException_WhenWebsiteUrlIsMalformed()
    {
        // --- 1. ARRANGE ---
        var command = new RegisterProviderCommand
        {
            Email = "invalid.url@academicgateway.com",
            Username = "badurlprovider",
            Password = "SecurePassword123!",
            OrganizationName = "Innovate Academy",
            Industry = "EdTech",
            WebsiteUrl = "ftp://malicious-site.com" // Violates regex (must start with http or https)
        };

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(command);

        // --- 3. ASSERT ---
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Should_ThrowValidationException_WhenRequiredFieldsAreMissing()
    {
        // --- 1. ARRANGE ---
        var command = new RegisterProviderCommand
        {
            Email = "missing.fields@academicgateway.com",
            Username = "missingfields",
            Password = "123", // Too short (minimum 6 characters)
            OrganizationName = "", // Required field
            Industry = "" // Required field
        };

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(command);

        // --- 3. ASSERT ---
        await act.Should().ThrowAsync<ValidationException>();
    }
}
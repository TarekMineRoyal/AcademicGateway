using AcademicGateway.Application.Features.Providers.Commands.RegisterProvider;
using AcademicGateway.Application.Features.Providers.Queries.GetProviderProfile;
using FluentAssertions;
using IntegrationTests.Infrastructure;
using Xunit;

namespace AcademicGateway.IntegrationTests.Subdomains.Providers.Queries;

/// <summary>
/// Integration tests verifying lookups, profile data mapping accuracy, and error 
/// boundaries inside the GetProviderProfile query pipeline handler loop.
/// </summary>
[Collection("SharedDatabase")]
public class GetProviderProfileTests : BaseIntegrationTest
{
    public GetProviderProfileTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Ensures that querying for a corporate provider profile using an active, verified identifier 
    /// returns the appropriate structural DTO matching the persisted aggregate fields.
    /// </summary>
    [Fact]
    public async Task Should_ReturnProviderProfile_WhenUserIdExists()
    {
        // --- 1. ARRANGE ---
        var registerCommand = new RegisterProviderCommand
        {
            Email = "profile.provider@academicgateway.com",
            Username = "profileprovider",
            Password = "SecurePassword123!",
            CompanyName = "Global Tech Solutions",
            CompanyDescription = "Enterprise IT and Academic Integration Services",
            WebsiteUrl = "https://globaltech.com"
        };

        // Execution returns a strongly-typed Guid key identifier
        Guid providerId = await SendAsync(registerCommand);
        var query = new GetProviderProfileQuery(providerId);

        // --- 2. ACT ---
        var result = await SendAsync(query);

        // --- 3. ASSERT ---
        result.Should().NotBeNull();

        // Aligns with the standardized DTO 'Id' property contract naming conventions
        result.Id.Should().Be(providerId);
        result.CompanyName.Should().Be("Global Tech Solutions");
        result.CompanyDescription.Should().Be("Enterprise IT and Academic Integration Services");
        result.WebsiteUrl.Should().Be("https://globaltech.com");
        result.IsVerified.Should().BeFalse(); // New providers must always default to unverified
    }

    /// <summary>
    /// Ensures that dispatching a lookup query with a non-existent tracking reference 
    /// short-circuits gracefully at the data layer, throwing a <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Should_ThrowKeyNotFoundException_WhenUserIdDoesNotExist()
    {
        // --- 1. ARRANGE ---
        // Instantiated using a clean, unmapped random Guid value to challenge lookups
        var query = new GetProviderProfileQuery(Guid.NewGuid());

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(query);

        // --- 3. ASSERT ---
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
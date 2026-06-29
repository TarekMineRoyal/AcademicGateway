using AcademicGateway.Application.Features.Users.Queries.GetProviderProfile;
using AcademicGateway.Application.Features.Users.Commands.RegisterProvider;
using FluentAssertions;
using Xunit;

namespace AcademicGateway.IntegrationTests.Features.Users.Queries.GetProviderProfile;

public class GetProviderProfileTests : BaseIntegrationTest
{
    public GetProviderProfileTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Should_ReturnProviderProfile_WhenUserIdExists()
    {
        // --- 1. ARRANGE ---
        var registerCommand = new RegisterProviderCommand
        {
            Email = "profile.provider@academicgateway.com",
            Username = "profileprovider",
            Password = "SecurePassword123!",
            OrganizationName = "Global Tech Solutions",
            Industry = "IT Services",
            WebsiteUrl = "https://globaltech.com"
        };

        var userId = await SendAsync(registerCommand);
        var query = new GetProviderProfileQuery(userId);

        // --- 2. ACT ---
        var result = await SendAsync(query);

        // --- 3. ASSERT ---
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.OrganizationName.Should().Be("Global Tech Solutions");
        result.Industry.Should().Be("IT Services");
        result.WebsiteUrl.Should().Be("https://globaltech.com");
        result.IsVerified.Should().BeFalse();
    }

    [Fact]
    public async Task Should_ThrowKeyNotFoundException_WhenUserIdDoesNotExist()
    {
        // --- 1. ARRANGE ---
        var query = new GetProviderProfileQuery("non-existent-provider-id");

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(query);

        // --- 3. ASSERT ---
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
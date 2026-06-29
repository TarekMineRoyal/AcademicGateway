using FluentAssertions;
using System.Net;
using Xunit;

namespace AcademicGateway.IntegrationTests.Features.Profiles;

public class ProfileAuthorizationTests : BaseIntegrationTest
{
    public ProfileAuthorizationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetStudentProfile_ShouldReturnUnauthorized_WhenUserIsAnonymous()
    {
        // --- 1. ARRANGE ---
        var client = GetAnonymousClient();

        // --- 2. ACT ---
        var response = await client.GetAsync("api/profiles/student", TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfessorProfile_ShouldReturnUnauthorized_WhenUserIsAnonymous()
    {
        // --- 1. ARRANGE ---
        var client = GetAnonymousClient();

        // --- 2. ACT ---
        var response = await client.GetAsync("api/profiles/professor", TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProviderProfile_ShouldReturnUnauthorized_WhenUserIsAnonymous()
    {
        // --- 1. ARRANGE ---
        // Validate authentication guard on provider profile query endpoint
        var client = GetAnonymousClient();

        // --- 2. ACT ---
        var response = await client.GetAsync("api/profiles/provider", TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
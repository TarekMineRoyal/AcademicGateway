using FluentAssertions;
using IntegrationTests.Infrastructure;
using System.Net;
using Xunit;

namespace AcademicGateway.IntegrationTests.Subdomains.Professors.Queries;

/// <summary>
/// Integration tests verifying security boundaries, identity validation guards, 
/// and route authorization policies for the Professor aggregate query endpoints.
/// </summary>
[Collection("SharedDatabase")]
public class ProfessorProfileAuthorizationTests : BaseIntegrationTest
{
    public ProfessorProfileAuthorizationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Ensures that an unauthenticated anonymous user attempting to fetch professor profile details 
    /// is explicitly blocked at the API gateway layer with a 401 Unauthorized response status code.
    /// </summary>
    [Fact]
    public async Task GetProfessorProfile_ShouldReturnUnauthorized_WhenUserIsAnonymous()
    {
        // --- 1. ARRANGE ---
        // Request an HTTP client instance explicitly configured without authorization headers
        var anonymousClient = GetAnonymousClient();

        // --- 2. ACT ---
        // Dispatch an unauthenticated request to the refactored profile endpoint route
        var response = await anonymousClient.GetAsync("api/professors/profile", TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        // Verify that the ASP.NET Core security pipeline terminates the request safely
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
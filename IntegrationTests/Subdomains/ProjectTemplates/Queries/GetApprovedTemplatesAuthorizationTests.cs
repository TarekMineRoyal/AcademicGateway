using FluentAssertions;
using IntegrationTests.Infrastructure;
using System.Net;
using Xunit;

namespace IntegrationTests.Subdomains.ProjectTemplates.Queries;

/// <summary>
/// Infrastructure integration tests verifying perimeter security boundaries and global authentication 
/// guards protecting the approved project template collection retrieval endpoint.
/// </summary>
[Collection("SharedDatabase")]
public class GetApprovedTemplatesAuthorizationTests : BaseIntegrationTest
{
    public GetApprovedTemplatesAuthorizationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Ensures that an anonymous unauthenticated user attempting to query the approved template list 
    /// is explicitly blocked at the API gateway layer with a 401 Unauthorized response status code.
    /// </summary>
    [Fact]
    public async Task GetApprovedTemplates_ShouldReturnUnauthorized_WhenUserIsAnonymous()
    {
        // --- 1. ARRANGE ---
        // Request an HTTP client instance explicitly configured without authorization headers
        var anonymousClient = GetAnonymousClient();

        // --- 2. ACT ---
        // Dispatch an unauthenticated GET request to the refactored approved templates route
        var response = await anonymousClient.GetAsync("api/templates/approved", TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        // Verify that the ASP.NET Core security pipeline terminates the request safely
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Ensures that any authenticated user (simulated here via a Student identity context) 
    /// safely passes the baseline perimeter guard and receives a successful 200 OK status code.
    /// </summary>
    [Fact]
    public async Task GetApprovedTemplates_ShouldReturnSuccess_WhenUserIsAuthenticatedAsStudent()
    {
        // --- 1. ARRANGE ---
        // Request an HTTP client pre-configured with valid student authorization tokens
        var studentClient = GetStudentClient();

        // --- 2. ACT ---
        // Dispatch an authenticated request to the approved templates route loop
        var response = await studentClient.GetAsync("api/templates/approved", TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        // Verify the user passes the authentication guard successfully
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
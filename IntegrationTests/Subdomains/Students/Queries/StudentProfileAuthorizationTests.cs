using FluentAssertions;
using IntegrationTests.Infrastructure;
using System.Net;
using Xunit;

namespace AcademicGateway.IntegrationTests.Subdomains.Students.Queries;

/// <summary>
/// Integration tests verifying security boundaries, identity validation guards, 
/// and route authorization policies for the Student aggregate query endpoints.
/// </summary>
[Collection("SharedDatabase")]
public class StudentProfileAuthorizationTests : BaseIntegrationTest
{
    public StudentProfileAuthorizationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Ensures that an unauthenticated anonymous user attempting to fetch student profile details 
    /// is explicitly blocked at the API gateway layer with a 401 Unauthorized response status code.
    /// </summary>
    [Fact]
    public async Task GetStudentProfile_ShouldReturnUnauthorized_WhenUserIsAnonymous()
    {
        // --- 1. ARRANGE ---
        // Request an HTTP client instance explicitly configured without authorization headers
        var anonymousClient = GetAnonymousClient();

        // --- 2. ACT ---
        // Dispatch an unauthenticated request to the refactored profile endpoint route
        var response = await anonymousClient.GetAsync("api/students/profile", TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        // Verify that the ASP.NET Core security pipeline terminates the request safely
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
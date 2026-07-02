using AcademicGateway.Application.Features.TechSupportAccounts.Commands.CreateTechSupportAccount;
using FluentAssertions;
using IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace IntegrationTests.Subdomains.TechSupportAccounts.Commands;

/// <summary>
/// Infrastructure integration tests verifying perimeter security boundaries and Role-Based 
/// Access Control (RBAC) attributes protecting the technical support account creation endpoint.
/// </summary>
[Collection("SharedDatabase")]
public class CreateTechSupportAuthorizationTests : BaseIntegrationTest
{
    public CreateTechSupportAuthorizationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Ensures that an anonymous unauthenticated user attempting to create a technical support account 
    /// is explicitly blocked at the API gateway layer with a 401 Unauthorized response status code.
    /// </summary>
    [Fact]
    public async Task CreateTechSupport_ShouldReturnUnauthorized_WhenUserIsAnonymous()
    {
        // --- 1. ARRANGE ---
        var anonymousClient = GetAnonymousClient();

        // Build payload matching the refactored command parameters (FullName removed)
        var payload = new CreateTechSupportAccountCommand
        {
            Email = "anon.tech@provider.com",
            Password = "SecurePassword123!"
        };

        // --- 2. ACT ---
        var response = await anonymousClient.PostAsJsonAsync("api/providers/tech-support", payload, TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Ensures that an authenticated user holding only the Student role is denied access 
    /// with a 403 Forbidden response status code when attempting to create provider technical support profiles.
    /// </summary>
    [Fact]
    public async Task CreateTechSupport_ShouldReturnForbidden_WhenUserIsStudent()
    {
        // --- 1. ARRANGE ---
        var studentClient = GetStudentClient();

        // Build payload matching the refactored command parameters (FullName removed)
        var payload = new CreateTechSupportAccountCommand
        {
            Email = "student.tech@provider.com",
            Password = "SecurePassword123!"
        };

        // --- 2. ACT ---
        var response = await studentClient.PostAsJsonAsync("api/providers/tech-support", payload, TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Ensures that an authenticated user holding the faculty Reviewer role is denied access 
    /// with a 403 Forbidden response status code when attempting to execute technical support provisioning actions.
    /// </summary>
    [Fact]
    public async Task CreateTechSupport_ShouldReturnForbidden_WhenUserIsReviewer()
    {
        // --- 1. ARRANGE ---
        var reviewerClient = GetReviewerClient();

        // Build payload matching the refactored command parameters (FullName removed)
        var payload = new CreateTechSupportAccountCommand
        {
            Email = "reviewer.tech@provider.com",
            Password = "SecurePassword123!"
        };

        // --- 2. ACT ---
        var response = await reviewerClient.PostAsJsonAsync("api/providers/tech-support", payload, TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
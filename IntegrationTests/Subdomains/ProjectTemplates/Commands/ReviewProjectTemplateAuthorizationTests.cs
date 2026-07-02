using FluentAssertions;
using IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace IntegrationTests.Subdomains.ProjectTemplates.Commands;

/// <summary>
/// Infrastructure integration tests verifying perimeter security boundaries and Role-Based 
/// Access Control (RBAC) attributes protecting the project template evaluation endpoint.
/// </summary>
[Collection("SharedDatabase")]
public class ReviewProjectTemplateAuthorizationTests : BaseIntegrationTest
{
    public ReviewProjectTemplateAuthorizationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Ensures that an anonymous unauthenticated user attempting to review an active project proposal
    /// is explicitly blocked at the API gateway perimeter with a 401 Unauthorized response status code.
    /// </summary>
    [Fact]
    public async Task ReviewTemplate_ShouldReturnUnauthorized_WhenUserIsAnonymous()
    {
        // --- 1. ARRANGE ---
        var anonymousClient = GetAnonymousClient();
        var templateId = Guid.NewGuid();

        // Inline anonymous type payload matching the feature slice review contract schema
        var payload = new { IsApproved = true, RejectionReason = (string?)null };

        // --- 2. ACT ---
        var response = await anonymousClient.PostAsJsonAsync(
            $"api/reviewers/templates/{templateId}/review",
            payload,
            TestContext.Current.CancellationToken
        );

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Ensures that an authenticated user holding only the Student role is denied access 
    /// with a 403 Forbidden response status code when trying to execute template evaluations.
    /// </summary>
    [Fact]
    public async Task ReviewTemplate_ShouldReturnForbidden_WhenUserIsStudent()
    {
        // --- 1. ARRANGE ---
        var studentClient = GetStudentClient();
        var templateId = Guid.NewGuid();
        var payload = new { IsApproved = true, RejectionReason = (string?)null };

        // --- 2. ACT ---
        var response = await studentClient.PostAsJsonAsync(
            $"api/reviewers/templates/{templateId}/review",
            payload,
            TestContext.Current.CancellationToken
        );

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Ensures that an authenticated corporate Provider user is denied access with a 403 Forbidden 
    /// response status code when attempting to evaluate or approve template entries.
    /// </summary>
    [Fact]
    public async Task ReviewTemplate_ShouldReturnForbidden_WhenUserIsProvider()
    {
        // --- 1. ARRANGE ---
        var providerClient = GetProviderClient();
        var templateId = Guid.NewGuid();
        var payload = new { IsApproved = true, RejectionReason = (string?)null };

        // --- 2. ACT ---
        var response = await providerClient.PostAsJsonAsync(
            $"api/reviewers/templates/{templateId}/review",
            payload,
            TestContext.Current.CancellationToken
        );

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
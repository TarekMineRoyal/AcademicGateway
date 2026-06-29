using AcademicGateway.Api.Controllers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace AcademicGateway.IntegrationTests.Features.Reviewers;

public class ReviewerAuthorizationTests : BaseIntegrationTest
{
    public ReviewerAuthorizationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    // =========================================================================
    // Endpoint 1: POST /api/reviewers/applications/{id}/review
    // =========================================================================

    [Fact]
    public async Task ReviewApplication_ShouldReturnUnauthorized_WhenUserIsAnonymous()
    {
        // --- 1. ARRANGE ---
        var client = GetAnonymousClient();
        var applicationId = Guid.NewGuid();
        var payload = new ReviewApplicationRequest(IsApproved: true, RejectionReason: null);

        // --- 2. ACT ---
        var response = await client.PostAsJsonAsync($"api/reviewers/applications/{applicationId}/review", payload, TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ReviewApplication_ShouldReturnForbidden_WhenUserIsStudent()
    {
        // --- 1. ARRANGE ---
        var client = GetStudentClient();
        var applicationId = Guid.NewGuid();
        var payload = new ReviewApplicationRequest(IsApproved: true, RejectionReason: null);

        // --- 2. ACT ---
        var response = await client.PostAsJsonAsync($"api/reviewers/applications/{applicationId}/review", payload, TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ReviewApplication_ShouldReturnForbidden_WhenUserIsProvider()
    {
        // --- 1. ARRANGE ---
        var client = GetProviderClient();
        var applicationId = Guid.NewGuid();
        var payload = new ReviewApplicationRequest(IsApproved: true, RejectionReason: null);

        // --- 2. ACT ---
        var response = await client.PostAsJsonAsync($"api/reviewers/applications/{applicationId}/review", payload, TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // =========================================================================
    // Endpoint 2: POST /api/reviewers/templates/{id}/review
    // =========================================================================

    [Fact]
    public async Task ReviewTemplate_ShouldReturnUnauthorized_WhenUserIsAnonymous()
    {
        // --- 1. ARRANGE ---
        var client = GetAnonymousClient();
        var templateId = Guid.NewGuid();
        var payload = new ReviewTemplateRequest(IsApproved: true, RejectionReason: null);

        // --- 2. ACT ---
        var response = await client.PostAsJsonAsync($"api/reviewers/templates/{templateId}/review", payload, TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ReviewTemplate_ShouldReturnForbidden_WhenUserIsStudent()
    {
        // --- 1. ARRANGE ---
        var client = GetStudentClient();
        var templateId = Guid.NewGuid();
        var payload = new ReviewTemplateRequest(IsApproved: true, RejectionReason: null);

        // --- 2. ACT ---
        var response = await client.PostAsJsonAsync($"api/reviewers/templates/{templateId}/review", payload, TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ReviewTemplate_ShouldReturnForbidden_WhenUserIsProvider()
    {
        // --- 1. ARRANGE ---
        var client = GetProviderClient();
        var templateId = Guid.NewGuid();
        var payload = new ReviewTemplateRequest(IsApproved: true, RejectionReason: null);

        // --- 2. ACT ---
        var response = await client.PostAsJsonAsync($"api/reviewers/templates/{templateId}/review", payload, TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
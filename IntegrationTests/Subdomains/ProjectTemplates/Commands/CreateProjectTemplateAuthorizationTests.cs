using AcademicGateway.Api.Features.ProjectTemplates.Create;
using FluentAssertions;
using IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace IntegrationTests.Subdomains.ProjectTemplates.Commands;

/// <summary>
/// Infrastructure integration tests verifying perimeter security boundaries and Role-Based 
/// Access Control (RBAC) attributes protecting the project template creation endpoint.
/// </summary>
[Collection("SharedDatabase")]
public class CreateProjectTemplateAuthorizationTests : BaseIntegrationTest
{
    public CreateProjectTemplateAuthorizationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Ensures that an anonymous unauthenticated user attempting to propose a new template 
    /// is explicitly blocked at the API gateway layer with a 401 Unauthorized status.
    /// </summary>
    [Fact]
    public async Task CreateTemplate_ShouldReturnUnauthorized_WhenUserIsAnonymous()
    {
        // --- 1. ARRANGE ---
        var anonymousClient = GetAnonymousClient();

        // Build payload matching the refactored controller request record schema
        var payload = new CreateTemplateRequest(
            Title: "Unauthorized Curriculum",
            Description: "An anonymous template proposal attempt bypassing authentication parameters.",
            SkillIds: new List<Guid> { Guid.NewGuid() }
        );

        // --- 2. ACT ---
        var response = await anonymousClient.PostAsJsonAsync("api/templates", payload, TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Ensures that an authenticated user holding only the Student role is denied access 
    /// with a 403 Forbidden status when attempting to propose a corporate partner template.
    /// </summary>
    [Fact]
    public async Task CreateTemplate_ShouldReturnForbidden_WhenUserIsStudent()
    {
        // --- 1. ARRANGE ---
        var studentClient = GetStudentClient();
        var payload = new CreateTemplateRequest(
            Title: "Student Breached Template",
            Description: "An invalid request trying to force creation using a non-permitted student identity context.",
            SkillIds: new List<Guid> { Guid.NewGuid() }
        );

        // --- 2. ACT ---
        var response = await studentClient.PostAsJsonAsync("api/templates", payload, TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Ensures that an authenticated user holding the faculty Reviewer role is denied access 
    /// with a 403 Forbidden status when attempting to execute actions reserved strictly for Providers.
    /// </summary>
    [Fact]
    public async Task CreateTemplate_ShouldReturnForbidden_WhenUserIsReviewer()
    {
        // --- 1. ARRANGE ---
        var reviewerClient = GetReviewerClient();
        var payload = new CreateTemplateRequest(
            Title: "Reviewer Breached Template",
            Description: "An administrative user trying to spawn a blueprint record instead of evaluating an existing queue entry.",
            SkillIds: new List<Guid> { Guid.NewGuid() }
        );

        // --- 2. ACT ---
        var response = await reviewerClient.PostAsJsonAsync("api/templates", payload, TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
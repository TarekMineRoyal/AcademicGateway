using AcademicGateway.Api.Controllers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace AcademicGateway.IntegrationTests.Features.ProjectTemplates;

public class ProjectTemplateAuthorizationTests : BaseIntegrationTest
{
    public ProjectTemplateAuthorizationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateTemplate_ShouldReturnUnauthorized_WhenUserIsAnonymous()
    {
        // --- 1. ARRANGE ---
        var client = GetAnonymousClient();
        var payload = new ApiCreateTemplateRequest(
            Title: "Unauthorized Curriculum",
            Description: "An anonymous template proposal attempt.",
            ExpectedDurationWeeks: 8,
            SkillIds: new List<Guid> { Guid.NewGuid() }
        );

        // --- 2. ACT ---
        var response = await client.PostAsJsonAsync("api/templates", payload, TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTemplate_ShouldReturnForbidden_WhenUserIsStudent()
    {
        // --- 1. ARRANGE ---
        var client = GetStudentClient();
        var payload = new ApiCreateTemplateRequest(
            Title: "Student Breached Template",
            Description: "A student trying to sneak past corporate role guards.",
            ExpectedDurationWeeks: 6,
            SkillIds: new List<Guid> { Guid.NewGuid() }
        );

        // --- 2. ACT ---
        var response = await client.PostAsJsonAsync("api/templates", payload, TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateTemplate_ShouldReturnForbidden_WhenUserIsReviewer()
    {
        // --- 1. ARRANGE ---
        var client = GetReviewerClient();
        var payload = new ApiCreateTemplateRequest(
            Title: "Reviewer Breached Template",
            Description: "A reviewer trying to initiate a template instead of evaluating one.",
            ExpectedDurationWeeks: 12,
            SkillIds: new List<Guid> { Guid.NewGuid() }
        );

        // --- 2. ACT ---
        var response = await client.PostAsJsonAsync("api/templates", payload, TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetApprovedTemplates_ShouldReturnUnauthorized_WhenUserIsAnonymous()
    {
        // --- 1. ARRANGE ---
        var client = GetAnonymousClient();

        // --- 2. ACT ---
        var response = await client.GetAsync("api/templates/approved", TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetApprovedTemplates_ShouldReturnSuccess_WhenUserIsAuthenticatedAsStudent()
    {
        // --- 1. ARRANGE ---
        // Verify positive authorization mapping for general [Authorize] endpoints
        var client = GetStudentClient();

        // --- 2. ACT ---
        var response = await client.GetAsync("api/templates/approved", TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
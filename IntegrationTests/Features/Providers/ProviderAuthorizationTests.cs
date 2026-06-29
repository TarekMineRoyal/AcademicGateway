using AcademicGateway.Api.Controllers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace AcademicGateway.IntegrationTests.Features.Providers;

public class ProviderAuthorizationTests : BaseIntegrationTest
{
    public ProviderAuthorizationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    // =========================================================================
    // Endpoint 1: POST /api/providers/applications (Submit Application)
    // =========================================================================

    [Fact]
    public async Task SubmitApplication_ShouldReturnUnauthorized_WhenUserIsAnonymous()
    {
        // --- 1. ARRANGE ---
        var client = GetAnonymousClient();
        var payload = new ApiSubmitApplicationRequest(
            CompanyDetails: "Anonymous EdTech Startup",
            VerificationDocumentsUrl: "https://docs.gateway.com/anon-verify.pdf"
        );

        // --- 2. ACT ---
        var response = await client.PostAsJsonAsync("api/providers/applications", payload, TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SubmitApplication_ShouldReturnForbidden_WhenUserIsStudent()
    {
        // --- 1. ARRANGE ---
        var client = GetStudentClient();
        var payload = new ApiSubmitApplicationRequest(
            CompanyDetails: "Malicious Student Inc",
            VerificationDocumentsUrl: "https://docs.gateway.com/student-bypass.pdf"
        );

        // --- 2. ACT ---
        var response = await client.PostAsJsonAsync("api/providers/applications", payload, TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SubmitApplication_ShouldReturnForbidden_WhenUserIsReviewer()
    {
        // --- 1. ARRANGE ---
        var client = GetReviewerClient();
        var payload = new ApiSubmitApplicationRequest(
            CompanyDetails: "Self-Reviewing Enterprise",
            VerificationDocumentsUrl: "https://docs.gateway.com/reviewer-bypass.pdf"
        );

        // --- 2. ACT ---
        var response = await client.PostAsJsonAsync("api/providers/applications", payload, TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // =========================================================================
    // Endpoint 2: POST /api/providers/tech-support (Create Tech Support)
    // =========================================================================

    [Fact]
    public async Task CreateTechSupport_ShouldReturnUnauthorized_WhenUserIsAnonymous()
    {
        // --- 1. ARRANGE ---
        var client = GetAnonymousClient();
        var payload = new ApiCreateTechSupportRequest(
            Email: "anon.tech@provider.com",
            Password: "SecurePassword123!",
            FullName: "Anonymous Support Profile"
        );

        // --- 2. ACT ---
        var response = await client.PostAsJsonAsync("api/providers/tech-support", payload, TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTechSupport_ShouldReturnForbidden_WhenUserIsStudent()
    {
        // --- 1. ARRANGE ---
        var client = GetStudentClient();
        var payload = new ApiCreateTechSupportRequest(
            Email: "student.tech@provider.com",
            Password: "SecurePassword123!",
            FullName: "Student Trainee Account"
        );

        // --- 2. ACT ---
        var response = await client.PostAsJsonAsync("api/providers/tech-support", payload, TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateTechSupport_ShouldReturnForbidden_WhenUserIsReviewer()
    {
        // --- 1. ARRANGE ---
        var client = GetReviewerClient();
        var payload = new ApiCreateTechSupportRequest(
            Email: "reviewer.tech@provider.com",
            Password: "SecurePassword123!",
            FullName: "Reviewer Audit Account"
        );

        // --- 2. ACT ---
        var response = await client.PostAsJsonAsync("api/providers/tech-support", payload, TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
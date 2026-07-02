using AcademicGateway.Application.Features.ProviderApplications.Commands.SubmitProviderApplication;
using FluentAssertions;
using IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace AcademicGateway.IntegrationTests.CrossCutting.Authorization;

/// <summary>
/// Infrastructure integration tests verifying perimeter security boundaries and Role-Based 
/// Access Control (RBAC) attributes protecting the provider onboarding application submission endpoint.
/// </summary>
[Collection("SharedDatabase")]
public class SubmitProviderApplicationAuthorizationTests : BaseIntegrationTest
{
    public SubmitProviderApplicationAuthorizationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Ensures that an anonymous unauthenticated user attempting to submit an onboarding application
    /// is explicitly blocked at the API gateway perimeter with a 401 Unauthorized status.
    /// </summary>
    [Fact]
    public async Task SubmitApplication_ShouldReturnUnauthorized_WhenUserIsAnonymous()
    {
        // --- 1. ARRANGE ---
        var anonymousClient = GetAnonymousClient();

        // Build payload matching the refactored feature slice command schema parameters
        var payload = new SubmitProviderApplicationCommand
        {
            CompanyDetails = "Anonymous EdTech Startup Application Metadata",
            VerificationDocumentsUrl = "https://docs.gateway.com/anon-verify.pdf"
        };

        // --- 2. ACT ---
        var response = await anonymousClient.PostAsJsonAsync("api/providers/applications", payload, TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Ensures that an authenticated user holding only the Student role is denied access
    /// with a 403 Forbidden status when attempting to execute provider onboarding actions.
    /// </summary>
    [Fact]
    public async Task SubmitApplication_ShouldReturnForbidden_WhenUserIsStudent()
    {
        // --- 1. ARRANGE ---
        var studentClient = GetStudentClient();
        var payload = new SubmitProviderApplicationCommand
        {
            CompanyDetails = "Malicious Student Bypassing Registration Filters",
            VerificationDocumentsUrl = "https://docs.gateway.com/student-bypass.pdf"
        };

        // --- 2. ACT ---
        var response = await studentClient.PostAsJsonAsync("api/providers/applications", payload, TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Ensures that an authenticated user holding the administrative Reviewer role is denied access
    /// with a 403 Forbidden status when attempting to submit an onboarding application.
    /// </summary>
    [Fact]
    public async Task SubmitApplication_ShouldReturnForbidden_WhenUserIsReviewer()
    {
        // --- 1. ARRANGE ---
        var reviewerClient = GetReviewerClient();
        var payload = new SubmitProviderApplicationCommand
        {
            CompanyDetails = "Self-Reviewing Enterprise Intentional Conflict Entry",
            VerificationDocumentsUrl = "https://docs.gateway.com/reviewer-bypass.pdf"
        };

        // --- 2. ACT ---
        var response = await reviewerClient.PostAsJsonAsync("api/providers/applications", payload, TestContext.Current.CancellationToken);

        // --- 3. ASSERT ---
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
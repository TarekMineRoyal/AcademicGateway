using AcademicGateway.Application.Features.ProjectTemplates.Queries.GetApprovedTemplates;
using AcademicGateway.Application.Features.Providers.Commands.RegisterProvider;
using AcademicGateway.Domain.ProjectTemplates;
using FluentAssertions;
using IntegrationTests.Infrastructure;
using Xunit;

namespace AcademicGateway.IntegrationTests.Subdomains.ProjectTemplates.Queries;

/// <summary>
/// Integration tests verifying project proposal filtering and lookup read operations 
/// within the project templates subdomain boundaries.
/// </summary>
[Collection("SharedDatabase")]
public class GetApprovedTemplatesTests : BaseIntegrationTest
{
    public GetApprovedTemplatesTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Ensures that dispatching a <see cref="GetApprovedTemplatesQuery"/> through the application pipeline
    /// extracts only those proposals that have successfully navigated the entire evaluation cycle 
    /// and reached an Approved state, strictly hiding all active drafts or rejected concepts.
    /// </summary>
    [Fact]
    public async Task Should_ReturnOnlyApprovedTemplates_And_ExcludeAllOtherStatuses()
    {
        // --- 1. ARRANGE ---
        // Register a corporate provider account to act as the proposal aggregate owner context
        var registerProviderCommand = new RegisterProviderCommand
        {
            Email = "template.test@academicgateway.com",
            Username = "templatetester",
            Password = "SecurePassword123!",
            CompanyName = "Test Corporation Solutions",
            CompanyDescription = "Academic Research Integration Partner Organization",
            WebsiteUrl = "https://test-corp-solutions.com"
        };
        Guid providerId = await SendAsync(registerProviderCommand);

        // Template 1: The Target Workflow Path (Navigates Draft -> UnderReview -> Approved)
        var approvedTemplate = new ProjectTemplate(
            title: "Approved Project",
            description: "This structural concept description is fully validated and ready for student selection.",
            providerId: providerId
        );
        approvedTemplate.SubmitForReview();
        approvedTemplate.Approve();
        await AddAsync(approvedTemplate);

        // Template 2: Boundary Case (Remains in initial default Draft state)
        var draftTemplate = new ProjectTemplate(
            title: "Draft Project",
            description: "Still actively refining constraints and core documentation deliverables.",
            providerId: providerId
        );
        await AddAsync(draftTemplate);

        // Template 3: Negative Path Case (Navigates Draft -> UnderReview -> RejectedPermanently)
        var rejectedTemplate = new ProjectTemplate(
            title: "Rejected Project",
            description: "Proposed infrastructure blueprint details lack clear milestone metrics.",
            providerId: providerId
        );
        rejectedTemplate.SubmitForReview();
        rejectedTemplate.RejectPermanently("Proposed design metrics lack required academic depth.");
        await AddAsync(rejectedTemplate);

        // Prepare the target CQRS lookup query
        var query = new GetApprovedTemplatesQuery();

        // --- 2. ACT ---
        // Route the query object through MediatR pipeline behaviors
        var result = await SendAsync(query);

        // --- 3. ASSERT ---
        result.Should().NotBeNull();

        // Assert that the query selectively filtered out the active draft and permanent rejection entries
        result.Should().HaveCount(1);

        // Verify the fields returned match the approved aggregate instance
        var returnedTemplate = result.First();
        returnedTemplate.Id.Should().Be(approvedTemplate.Id);
        returnedTemplate.Title.Should().Be("Approved Project");
    }
}
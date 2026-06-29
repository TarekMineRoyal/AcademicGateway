using AcademicGateway.Application.Features.ProjectTemplates.Queries.GetApprovedTemplates;
using AcademicGateway.Application.Features.Users.Commands.RegisterProvider;
using AcademicGateway.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace AcademicGateway.IntegrationTests.Features.ProjectTemplates.Queries.GetApprovedTemplates;

public class GetApprovedTemplatesTests : BaseIntegrationTest
{
    public GetApprovedTemplatesTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Should_ReturnOnlyApprovedTemplates_And_ExcludeAllOtherStatuses()
    {
        // --- 1. ARRANGE ---
        // Register the Provider (This creates the valid AspNetUser)
        var registerProviderCommand = new RegisterProviderCommand
        {
            Email = "template.test@academicgateway.com",
            Username = "templatetester",
            Password = "SecurePassword123!",
            OrganizationName = "Test Corporation",
            Industry = "Software"
        };
        var providerId = await SendAsync(registerProviderCommand);

        // FIX: Seed a Reviewer to satisfy the fk_project_templates_reviewers_approved_by_id constraint.
        // We reuse the 'providerId' as the IdentityUserId since we know it already exists in AspNetUsers!
        var reviewerId = Guid.NewGuid();
        var reviewer = new Reviewer(reviewerId, providerId, "Test Reviewer");
        await AddAsync(reviewer);

        // Template 1: The "Happy Path" (Approved)
        var approvedTemplate = new ProjectTemplate(providerId, "Approved Project", "This is ready for students.", 4);
        approvedTemplate.SubmitForReview();
        approvedTemplate.Approve(reviewerId);
        await AddAsync(approvedTemplate);

        // Template 2: Still a Draft
        var draftTemplate = new ProjectTemplate(providerId, "Draft Project", "Still working on the description.", 8);
        await AddAsync(draftTemplate);

        // Template 3: Rejected by a reviewer
        var rejectedTemplate = new ProjectTemplate(providerId, "Rejected Project", "Needs more detail.", 12);
        rejectedTemplate.SubmitForReview();
        rejectedTemplate.Reject(reviewerId, "Not enough academic rigor.");
        await AddAsync(rejectedTemplate);

        var query = new GetApprovedTemplatesQuery();

        // --- 2. ACT ---
        var result = await SendAsync(query);

        // --- 3. ASSERT ---
        result.Should().NotBeNull();

        // We seeded 3 templates, but the query should ONLY return 1
        result.Should().HaveCount(1);

        // Verify the one it returned is exactly the approved one
        var returnedTemplate = result.First();
        returnedTemplate.Id.Should().Be(approvedTemplate.Id);
        returnedTemplate.Title.Should().Be("Approved Project");
    }
}
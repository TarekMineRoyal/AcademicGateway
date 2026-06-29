using AcademicGateway.Application.Features.ProjectTemplates.Commands.ReviewTemplate;
using AcademicGateway.Application.Features.Users.Commands.RegisterProvider;
using AcademicGateway.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace AcademicGateway.IntegrationTests.Features.ProjectTemplates;

public class ProjectTemplateStateTests : BaseIntegrationTest
{
    public ProjectTemplateStateTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_WhenReviewerAttemptsToEvaluateDraftTemplate()
    {
        // --- 1. ARRANGE ---
        var registerProviderCommand = new RegisterProviderCommand
        {
            Email = "template.state@academicgateway.com",
            Username = "templatestateprov",
            Password = "SecurePassword123!",
            OrganizationName = "Template Guard LLC",
            Industry = "Education"
        };
        var providerId = await SendAsync(registerProviderCommand);

        var reviewerId = Guid.NewGuid();
        var reviewer = new Reviewer(reviewerId, providerId, "Template Auditor");
        await AddAsync(reviewer);

        // Intentionally create a template and save it in 'Draft' status (skipping SubmitForReview)
        var draftTemplate = new ProjectTemplate(providerId, "Illegal State Test", "Testing workflow bounds", 6);
        await AddAsync(draftTemplate);

        // Attempt to force a review directly on the draft
        var illegalReviewCommand = new ReviewProjectTemplateCommand
        {
            TemplateId = draftTemplate.Id,
            ReviewerIdentityUserId = providerId,
            IsApproved = true,
            RejectionReason = null
        };

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(illegalReviewCommand);

        // --- 3. ASSERT ---
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("Only pending templates can be approved.");
    }
}
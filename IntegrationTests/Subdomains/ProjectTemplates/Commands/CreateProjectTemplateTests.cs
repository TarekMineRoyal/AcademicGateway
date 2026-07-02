using AcademicGateway.Application.Features.ProjectTemplates.Commands.CreateProjectTemplate;
using AcademicGateway.Application.Features.Providers.Commands.RegisterProvider;
using AcademicGateway.Domain.Skills;
using AcademicGateway.Domain.Providers;
using AcademicGateway.Domain.Providers.Exceptions;
using FluentAssertions;
using Xunit;
using IntegrationTests.Infrastructure;

namespace AcademicGateway.IntegrationTests.Subdomains.ProjectTemplates.Commands;

/// <summary>
/// Integration tests verifying validation rules, entity mapping preconditions,
/// and security invariant guards for the project template creation command pipeline.
/// </summary>
[Collection("SharedDatabase")]
public class CreateProjectTemplateTests : BaseIntegrationTest
{
    public CreateProjectTemplateTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// Ensures that an unverified provider profile attempting to propose a new template 
    /// is explicitly blocked by domain rules, throwing a <see cref="ProviderNotVerifiedException"/>.
    /// </summary>
    [Fact]
    public async Task Should_ThrowProviderNotVerifiedException_WhenUnverifiedProviderAttemptsTemplateCreation()
    {
        // --- 1. ARRANGE ---
        // Register a corporate provider account profile context
        var registerProviderCommand = new RegisterProviderCommand
        {
            Email = "unverified.provider@academicgateway.com",
            Username = "unverifiedprov",
            Password = "SecurePassword123!",
            CompanyName = "Rogue Org Inc",
            CompanyDescription = "External Unverified Sponsoring Organization Cluster",
            WebsiteUrl = "https://rogue-org-inc.com"
        };
        Guid providerId = await SendAsync(registerProviderCommand);

        // Seed a standard technical skill lookup dependency using clean constructors
        var dummySkill = new Skill("Dummy Verification Competency");
        await AddAsync(dummySkill);

        // Prepare the creation command targeting the unverified firm profile
        var command = new CreateProjectTemplateCommand
        {
            ProviderId = providerId,
            Title = "Unauthorized Project Template",
            Description = "This blueprint proposal execution attempt must be completely dropped by domain guards.",
            SkillIds = new List<Guid> { dummySkill.Id }
        };

        // --- 2. ACT ---
        // Capture execution logic into a delegate vector for assertion parsing
        Func<Task> act = async () => await SendAsync(command);

        // --- 3. ASSERT ---
        // Verify the rich domain layer successfully intercepts the operation and enforces verification standing boundaries
        await act.Should().ThrowAsync<ProviderNotVerifiedException>();
    }

    /// <summary>
    /// Ensures that executing a template creation command with a non-existent provider ID 
    /// fails at the application handler boundary, throwing a <see cref="KeyNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Should_ThrowKeyNotFoundException_WhenProviderProfileDoesNotExist()
    {
        // --- 1. ARRANGE ---
        // Seed an active technical capability lookup row
        var dummySkill = new Skill("Valid Database Skill");
        await AddAsync(dummySkill);

        // Build a creation command referencing an unmapped arbitrary provider identifier Guid
        var command = new CreateProjectTemplateCommand
        {
            ProviderId = Guid.NewGuid(), // Simulates a missing profile reference target
            Title = "Ghost Template Integration Unit",
            Description = "No corporate organization profile matches this parameter payload inside the tracking schema.",
            SkillIds = new List<Guid> { dummySkill.Id }
        };

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(command);

        // --- 3. ASSERT ---
        // Verify the handler breaks gracefully with an explicit data missing warning
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
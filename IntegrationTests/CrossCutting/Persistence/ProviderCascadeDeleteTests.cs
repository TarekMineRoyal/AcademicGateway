using AcademicGateway.Application.Features.Providers.Commands.RegisterProvider;
using AcademicGateway.Domain.ProjectTemplates;
using AcademicGateway.Domain.Skills;
using AcademicGateway.Domain.Providers;
using AcademicGateway.Infrastructure.Persistence;
using AcademicGateway.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using IntegrationTests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcademicGateway.IntegrationTests.CrossCutting.Persistence;

/// <summary>
/// Integration tests verifying database persistence constraints and foreign key cascade rules
/// when corporate provider profiles are deleted from the system.
/// </summary>
[Collection("SharedDatabase")]
public class ProviderCascadeDeleteTests : BaseIntegrationTest
{
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderCascadeDeleteTests"/> class.
    /// </summary>
    /// <param name="factory">The centralized integration web application testing factory infrastructure context.</param>
    public ProviderCascadeDeleteTests(CustomWebApplicationFactory factory) : base(factory)
    {
        _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    /// <summary>
    /// Ensures that removing a <see cref="Provider"/> aggregate root from the persistence store 
    /// completely cascades to clear out related applications, templates, and required skill matrices.
    /// </summary>
    [Fact]
    public async Task DeletingProvider_ShouldCascadeDelete_ApplicationsTemplatesAndSkills()
    {
        // --- 1. ARRANGE ---
        // Register a new corporate provider via the command pipeline
        var providerCommand = new RegisterProviderCommand
        {
            Email = "provider.fullcascade@academicgateway.com",
            Username = "fullcascadeprov",
            Password = "SecurePassword123!",
            CompanyName = "Cascade Hub Corp",
            CompanyDescription = "Enterprise DevOps Training Infrastructure Solutions",
            WebsiteUrl = "https://cascade-hub.com"
        };
        Guid providerId = await SendAsync(providerCommand);

        // Seed a standard skill lookup following domain validation rules
        var skill = new Skill("Docker");
        await AddAsync(skill);

        // Instantiate operational dependencies using explicit domain constructor logic
        var providerApp = new ProviderApplication(
            providerId: providerId,
            companyDetails: "DevOps Core Training Profile Metadata Summary",
            verificationDocumentsUrl: "https://docs.com/devops.pdf",
            createdAt: DateTime.UtcNow
        );

        var projectTemplate = new ProjectTemplate(
            title: "CI/CD Pipeline Template",
            description: "Automate delivery workflows cleanly via containerized environments",
            providerId: providerId
        );

        // Map technical skill capabilities directly using behavioral methods on the aggregate root
        projectTemplate.AddSkill(skill.Id);

        // Provision the underlying security user identity rows for a dedicated tech support staff member
        var techSupportUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "techsupport.cascade@academicgateway.com",
            Email = "techsupport.cascade@academicgateway.com"
        };
        await AddAsync(techSupportUser);

        // Instantiate a technical support employee account profile bounded to their unique security credentials
        var techSupportAccount = new TechSupportAccount(
            id: techSupportUser.Id,
            staffNumber: "EMP-DEVOPS-992",
            supportTier: "Tier 3 Systems Admin"
        );

        // Persist the structured aggregate dependencies into the store layers
        await AddAsync(providerApp);
        await AddAsync(projectTemplate);
        await AddAsync(techSupportAccount);

        // Pre-assertion: Verify records exist in the database prior to deletion execution
        (await FindAsync<Provider>(providerId)).Should().NotBeNull();
        (await FindAsync<ProviderApplication>(providerApp.Id)).Should().NotBeNull();
        (await FindAsync<ProjectTemplate>(projectTemplate.Id)).Should().NotBeNull();
        (await FindAsync<TechSupportAccount>(techSupportAccount.Id)).Should().NotBeNull();

        // --- 2. ACT ---
        // Purge the corporate provider aggregate root through the relational database context
        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var providerEntity = await context.Providers.FindAsync(new object[] { providerId }, TestContext.Current.CancellationToken);
            providerEntity.Should().NotBeNull();

            context.Providers.Remove(providerEntity!);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // --- 3. ASSERT ---
        // Verify the provider aggregate root was completely removed
        (await FindAsync<Provider>(providerId)).Should().BeNull();

        // Verify cascading delete constraints cleanly purged the stateful application workflow record
        (await FindAsync<ProviderApplication>(providerApp.Id)).Should().BeNull();

        // Verify cascading rules removed proposed project templates and associated skill mapping intersections
        (await FindAsync<ProjectTemplate>(projectTemplate.Id)).Should().BeNull();
        (await FindAsync<ProjectTemplateSkill>(projectTemplate.Id, skill.Id)).Should().BeNull();

        // Note: TechSupportAccount remains active as its lifecycle binds independently to its specific ApplicationUser account
        (await FindAsync<TechSupportAccount>(techSupportAccount.Id)).Should().NotBeNull();
    }
}
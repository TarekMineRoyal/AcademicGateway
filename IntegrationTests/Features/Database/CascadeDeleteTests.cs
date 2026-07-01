using AcademicGateway.Application.Features.Users.Commands.RegisterProvider;
using AcademicGateway.Application.Features.Users.Commands.RegisterStudent;
using AcademicGateway.Infrastructure.Identity;
using AcademicGateway.Infrastructure.Persistence;
using Domain.Lookups;
using Domain.Models.Academic;
using Domain.ProjectTemplates;
using Domain.Providers;
using Domain.Students;
using Domain.SystemStaff;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AcademicGateway.IntegrationTests.Features.Database;

public class CascadeDeleteTests : BaseIntegrationTest
{
    private readonly IServiceScopeFactory _scopeFactory;

    public CascadeDeleteTests(CustomWebApplicationFactory factory) : base(factory)
    {
        _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    [Fact]
    public async Task DeletingIdentityUser_ShouldCascadeDelete_StudentProfileAndAllJunctionRecords()
    {
        // --- 1. ARRANGE ---
        var major = new Major { Id = Guid.NewGuid(), Name = "Data Science" };
        var specialty = new Specialty { Id = Guid.NewGuid(), Name = "Deep Learning", MajorId = major.Id };
        var skill = new Skill { Id = Guid.NewGuid(), Name = "PyTorch" };

        await AddAsync(major);
        await AddAsync(specialty);
        await AddAsync(skill);

        var command = new RegisterStudentCommand
        {
            Email = "identity.cascade@academicgateway.com",
            Username = "identitycascade",
            Password = "SecurePassword123!",
            GraduationYear = 2026,
            MajorIds = new List<Guid> { major.Id },
            SpecialtyIds = new List<Guid> { specialty.Id },
            SkillIds = new List<Guid> { skill.Id }
        };

        string userId = await SendAsync(command);

        // Confirm existence across the hierarchy
        (await FindAsync<Student>(userId)).Should().NotBeNull();
        (await FindAsync<StudentMajor>(userId, major.Id)).Should().NotBeNull();

        // --- 2. ACT ---
        // Purge parent ApplicationUser using standard Identity mechanisms with the responsive test cancellation token
        using (var scope = _scopeFactory.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByIdAsync(userId);
            user.Should().NotBeNull();

            var result = await userManager.DeleteAsync(user!);
            result.Succeeded.Should().BeTrue();
        }

        // --- 3. ASSERT ---
        // Verify profile was cleared by cascade delete configurations
        (await FindAsync<Student>(userId)).Should().BeNull();

        // Verify deep relational junction tables were safely removed
        (await FindAsync<StudentMajor>(userId, major.Id)).Should().BeNull();
        (await FindAsync<StudentSkill>(userId, skill.Id)).Should().BeNull();
        (await FindAsync<StudentSpecialty>(userId, specialty.Id)).Should().BeNull();
    }

    [Fact]
    public async Task DeletingProvider_ShouldCascadeDelete_ApplicationsTemplatesSkillsAndSupportAccounts()
    {
        // --- 1. ARRANGE ---
        var providerCommand = new RegisterProviderCommand
        {
            Email = "provider.fullcascade@academicgateway.com",
            Username = "fullcascadeprov",
            Password = "SecurePassword123!",
            OrganizationName = "Cascade Hub Corp",
            Industry = "DevOps",
            WebsiteUrl = "https://cascade-hub.com"
        };
        string providerUserId = await SendAsync(providerCommand);

        var skill = new Skill { Id = Guid.NewGuid(), Name = "Docker" };
        await AddAsync(skill);

        // Seed workflow elements
        var providerApp = new ProviderApplication(providerUserId, "DevOps Training Profile", "https://docs.com/devops.pdf");
        var projectTemplate = new ProjectTemplate(providerUserId, "CI/CD Pipeline Template", "Automate workflows", 4);
        await AddAsync(providerApp);
        await AddAsync(projectTemplate);

        // FIX: Instantiate join entity via the public constructor instead of object initializer properties
        var templateSkill = new ProjectTemplateSkill(projectTemplate.Id, skill.Id);
        await AddAsync(templateSkill);

        // Seed supporting account records
        var supportUser = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = "support_user_1", Email = "support1@cascade.com" };
        await AddAsync(supportUser);

        // FIX: Instantiate via public constructor to satisfy private setter protections
        var techSupport = new TechSupportAccount(providerUserId, supportUser.Id, "Tech Support Agent");
        await AddAsync(techSupport);

        // --- 2. ACT ---
        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var provider = await context.Providers.FindAsync(new object[] { providerUserId }, TestContext.Current.CancellationToken);
            context.Providers.Remove(provider!);

            // FIX: Pass TestContext.Current.CancellationToken to clear xUnit1051 warnings
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // --- 3. ASSERT ---
        (await FindAsync<Provider>(providerUserId)).Should().BeNull();
        (await FindAsync<ProviderApplication>(providerApp.Id)).Should().BeNull();
        (await FindAsync<ProjectTemplate>(projectTemplate.Id)).Should().BeNull();
        (await FindAsync<ProjectTemplateSkill>(projectTemplate.Id, skill.Id)).Should().BeNull();
        (await FindAsync<TechSupportAccount>(techSupport.Id)).Should().BeNull();
    }

    [Fact]
    public async Task DeletingReviewer_ShouldSetNull_OnTrackingForeignKeys()
    {
        // --- 1. ARRANGE ---
        var providerCommand = new RegisterProviderCommand
        {
            Email = "reviewer.nullify@academicgateway.com",
            Username = "nullifyprovider",
            Password = "SecurePassword123!",
            OrganizationName = "Nullify Systems",
            Industry = "Security",
            WebsiteUrl = "https://nullify-sec.com"
        };
        string providerUserId = await SendAsync(providerCommand);

        var reviewerIdentityUserId = Guid.NewGuid().ToString();
        var reviewerUser = new ApplicationUser { Id = reviewerIdentityUserId, UserName = "reviewer_edge", Email = "reviewer.edge@gateway.com" };
        await AddAsync(reviewerUser);

        var reviewerId = Guid.NewGuid();
        var reviewer = new Reviewer(reviewerId, reviewerIdentityUserId, "System Auditor");
        await AddAsync(reviewer);

        var providerApp = new ProviderApplication(providerUserId, "Security Verification", "https://docs.com/sec.pdf");
        providerApp.SubmitForReview();
        providerApp.Approve(reviewerId);

        var projectTemplate = new ProjectTemplate(providerUserId, "Penetration Testing Plan", "Security sandbox testing", 8);
        projectTemplate.SubmitForReview();
        projectTemplate.Approve(reviewerId);

        await AddAsync(providerApp);
        await AddAsync(projectTemplate);

        // --- 2. ACT ---
        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var rev = await context.Reviewers.FindAsync(new object[] { reviewerId }, TestContext.Current.CancellationToken);
            context.Reviewers.Remove(rev!);

            // FIX: Pass TestContext.Current.CancellationToken to clear xUnit1051 warnings
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // --- 3. ASSERT ---
        (await FindAsync<Reviewer>(reviewerId)).Should().BeNull();

        var updatedApp = await FindAsync<ProviderApplication>(providerApp.Id);
        updatedApp!.ReviewedById.Should().BeNull();

        var updatedTemplate = await FindAsync<ProjectTemplate>(projectTemplate.Id);
        updatedTemplate!.ApprovedById.Should().BeNull();
    }

    [Fact]
    public async Task DeletingMajor_ShouldCascadeDelete_ItsAssociatedSpecialties()
    {
        // --- 1. ARRANGE ---
        var major = new Major { Id = Guid.NewGuid(), Name = "Electrical Engineering" };
        var specialty1 = new Specialty { Id = Guid.NewGuid(), Name = "Telecommunications", MajorId = major.Id };
        var specialty2 = new Specialty { Id = Guid.NewGuid(), Name = "Embedded Systems", MajorId = major.Id };

        await AddAsync(major);
        await AddAsync(specialty1);
        await AddAsync(specialty2);

        // --- 2. ACT ---
        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var majorEntity = await context.Majors.FindAsync(new object[] { major.Id }, TestContext.Current.CancellationToken);
            context.Majors.Remove(majorEntity!);

            // FIX: Pass TestContext.Current.CancellationToken to clear xUnit1051 warnings
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // --- 3. ASSERT ---
        (await FindAsync<Major>(major.Id)).Should().BeNull();
        (await FindAsync<Specialty>(specialty1.Id)).Should().BeNull();
        (await FindAsync<Specialty>(specialty2.Id)).Should().BeNull();
    }
}
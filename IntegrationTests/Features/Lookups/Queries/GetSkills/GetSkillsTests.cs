using AcademicGateway.Application.Features.Lookups.Queries.GetSkills;
using AcademicGateway.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace AcademicGateway.IntegrationTests.Features.Lookups.Queries.GetSkills;

public class GetSkillsTests : BaseIntegrationTest
{
    public GetSkillsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Should_ReturnAllSeededSkills()
    {
        // --- 1. ARRANGE ---
        var skill1 = new Skill { Id = Guid.NewGuid(), Name = "Docker" };
        var skill2 = new Skill { Id = Guid.NewGuid(), Name = "PostgreSQL" };
        var skill3 = new Skill { Id = Guid.NewGuid(), Name = "ASP.NET Core" };

        await AddAsync(skill1);
        await AddAsync(skill2);
        await AddAsync(skill3);

        var query = new GetSkillsQuery();

        // --- 2. ACT ---
        var result = await SendAsync(query);

        // --- 3. ASSERT ---
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThanOrEqualTo(3); // Accounting for anything else in DB state
        result.Should().Contain(s => s.Id == skill1.Id && s.Name == "Docker");
        result.Should().Contain(s => s.Id == skill2.Id && s.Name == "PostgreSQL");
        result.Should().Contain(s => s.Id == skill3.Id && s.Name == "ASP.NET Core");
    }
}
using AcademicGateway.Application.Features.Lookups.Queries.GetMajors;
using AcademicGateway.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace AcademicGateway.IntegrationTests.Features.Lookups.Queries.GetMajors;

public class GetMajorsWithSpecialtiesTests : BaseIntegrationTest
{
    public GetMajorsWithSpecialtiesTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Should_ReturnMajorsWithTheirNestedSpecialties()
    {
        // --- 1. ARRANGE ---
        var major = new Major { Id = Guid.NewGuid(), Name = "Engineering" };
        await AddAsync(major);

        // Tie multiple specialties to the parent major
        var specialty1 = new Specialty { Id = Guid.NewGuid(), Name = "Mechanical Engineering", MajorId = major.Id };
        var specialty2 = new Specialty { Id = Guid.NewGuid(), Name = "Electrical Engineering", MajorId = major.Id };

        await AddAsync(specialty1);
        await AddAsync(specialty2);

        var query = new GetMajorsWithSpecialtiesQuery();

        // --- 2. ACT ---
        var result = await SendAsync(query);

        // --- 3. ASSERT ---
        result.Should().NotBeNull();

        // Find our target major in the output list
        var returnedMajor = result.FirstOrDefault(m => m.Id == major.Id);
        returnedMajor.Should().NotBeNull();
        returnedMajor!.Name.Should().Be("Engineering");

        // Verify the hierarchical nested collection mapped correctly 
        returnedMajor.Specialties.Should().HaveCount(2);
        returnedMajor.Specialties.Should().Contain(s => s.Id == specialty1.Id && s.Name == "Mechanical Engineering");
        returnedMajor.Specialties.Should().Contain(s => s.Id == specialty2.Id && s.Name == "Electrical Engineering");
    }
}
using AcademicGateway.Application.Features.Users.Queries.GetProfessorProfile;
using AcademicGateway.Application.Features.Users.Commands.RegisterProfessor;
using FluentAssertions;
using Xunit;

namespace AcademicGateway.IntegrationTests.Features.Users.Queries.GetProfessorProfile;

public class GetProfessorProfileTests : BaseIntegrationTest
{
    public GetProfessorProfileTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Should_ReturnProfessorProfile_WhenUserIdExists()
    {
        // --- 1. ARRANGE ---
        var registerCommand = new RegisterProfessorCommand
        {
            Email = "profile.professor@academicgateway.com",
            Username = "profileprofessor",
            Password = "SecurePassword123!",
            AcademicDepartment = "Data Science"
        };

        var userId = await SendAsync(registerCommand);
        var query = new GetProfessorProfileQuery(userId);

        // --- 2. ACT ---
        var result = await SendAsync(query);

        // --- 3. ASSERT ---
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.AcademicDepartment.Should().Be("Data Science");
    }

    [Fact]
    public async Task Should_ThrowKeyNotFoundException_WhenUserIdDoesNotExist()
    {
        // --- 1. ARRANGE ---
        var query = new GetProfessorProfileQuery("non-existent-professor-id");

        // --- 2. ACT ---
        Func<Task> act = async () => await SendAsync(query);

        // --- 3. ASSERT ---
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
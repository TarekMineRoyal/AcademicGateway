using IntegrationTests.Infrastructure;
using Xunit;

namespace AcademicGateway.IntegrationTests;

[CollectionDefinition("SharedDatabase")]
public class SharedTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}
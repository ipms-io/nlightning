namespace NLightning.Bolts.Tests.Docker;

using Fixtures;
using Utils;
using Xunit.Abstractions;

#pragma warning disable xUnit1033 // Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture
public class PostgresTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _postgresFixture;

    public PostgresTests(PostgresFixture fixture, ITestOutputHelper output)
    {
        _postgresFixture = fixture;
        Console.SetOut(new TestOutputWriter(output));
    }

    [Fact]
    public void Check_Postgres_Exists()
    {
        _postgresFixture.IsRunning();
    }
}
#pragma warning restore xUnit1033 // Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture
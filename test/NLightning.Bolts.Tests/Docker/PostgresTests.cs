using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLightning.Bolts.Tests.Db;
using ServiceStack.Text;

namespace NLightning.Bolts.Tests.Docker;

using Fixtures;
using Utils;
using Xunit.Abstractions;

#pragma warning disable xUnit1033 // Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture
[Collection("postgres")]
public class PostgresTests
{
    private readonly PostgresFixture _postgresFixture;
    private readonly ServiceProvider _serviceProvider;

    public PostgresTests(PostgresFixture fixture, ITestOutputHelper output)
    {
        _postgresFixture = fixture;

        Console.SetOut(new TestOutputWriter(output));

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddDbContextFactory<MockEfContext>(
            options =>
                options.UseNpgsql(_postgresFixture.DbConnectionString)
                    .EnableSensitiveDataLogging()
                    .UseSnakeCaseNamingConvention());
        serviceCollection.AddDbContext<MockEfContext>(ServiceLifetime.Transient);
        _serviceProvider = serviceCollection.BuildServiceProvider();
        var context = _serviceProvider.GetService<MockEfContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }

    [Fact]
    public void Check_Postgres_Exists()
    {
        Assert.True(_postgresFixture.IsRunning);
    }

    [Fact]
    public async Task TestDb()
    {
        var context = _serviceProvider.GetService<MockEfContext>();
        context.Xs.Count().PrintDump();

        context.AddRange(
            new MockEfContext.TableX(),
            new MockEfContext.TableX());
        context.SaveChanges();
        context.Xs.Count().PrintDump();
        context.AddRange(
            new MockEfContext.TableX(),
            new MockEfContext.TableX());
        context.Xs.Count().PrintDump();
        context.SaveChanges();
        context.Xs.Count().PrintDump();
    }

}
#pragma warning restore xUnit1033 // Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture
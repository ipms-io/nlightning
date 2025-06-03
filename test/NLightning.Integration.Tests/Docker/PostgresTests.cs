using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack.Text;
using Xunit.Abstractions;

namespace NLightning.Integration.Tests.Docker;

using Fixtures;
using Infrastructure.Persistence.Contexts;
using Utils;

#pragma warning disable xUnit1033 // Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture
[Collection("postgres")]
public class PostgresTests
{
    private readonly ServiceProvider _serviceProvider;

    public PostgresTests(PostgresFixture fixture, ITestOutputHelper output)
    {
        var postgresFixture = fixture;

        Console.SetOut(new TestOutputWriter(output));

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddDbContextFactory<NLightningDbContext>(
            options =>
                options.UseNpgsql(postgresFixture.DbConnectionString, x =>
                    {
                        x.MigrationsAssembly("NLightning.Infrastructure.Persistence.Postgres");
                    })
                    .EnableSensitiveDataLogging()
                    .UseSnakeCaseNamingConvention());
        serviceCollection.AddDbContext<NLightningDbContext>(x =>
        {
            x.UseNpgsql(postgresFixture.DbConnectionString, y =>
            {
                y.MigrationsAssembly("NLightning.Infrastructure.Persistence.Postgres");
            })
                .EnableSensitiveDataLogging()
                .UseSnakeCaseNamingConvention();
        }, ServiceLifetime.Transient);
        _serviceProvider = serviceCollection.BuildServiceProvider();
        var context = _serviceProvider.GetService<NLightningDbContext>() ?? throw new Exception($"Could not find a service provider for type {nameof(NLightningDbContext)}");

        //Wait until really ready
        while (!context.Database.CanConnect())
        {
            Task.Delay(100).Wait();
        }
        context.Database.Migrate();
    }

    [Fact]
    public Task TestDb()
    {
        var context = _serviceProvider.GetService<NLightningDbContext>() ?? throw new Exception("Context is null");
        context.Nodes.Count().PrintDump();

        context.Nodes.AddRange(
            new NLightningDbContext.Node(),
            new NLightningDbContext.Node());
        context.SaveChanges();
        context.Nodes.Count().PrintDump();
        context.AddRange(
            new NLightningDbContext.Node(),
            new NLightningDbContext.Node());
        context.Nodes.Count().PrintDump();
        context.SaveChanges();
        context.Nodes.Count().PrintDump();

        return Task.CompletedTask;
    }
}
#pragma warning restore xUnit1033 // Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLightning.Integration.Tests.Fixtures;
using ServiceStack.Text;
using Xunit.Abstractions;

namespace NLightning.Integration.Tests.Docker;

using Models;
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
        serviceCollection.AddDbContextFactory<NLightningContext>(
            options =>
                options.UseNpgsql(postgresFixture.DbConnectionString, x =>
                    {
                        x.MigrationsAssembly("NLightning.Models.Postgres");
                    })
                    .EnableSensitiveDataLogging()
                    .UseSnakeCaseNamingConvention());
        serviceCollection.AddDbContext<NLightningContext>(x =>
        {
            x.UseNpgsql(postgresFixture.DbConnectionString, y =>
            {
                y.MigrationsAssembly("NLightning.Models.Postgres");
            })
                .EnableSensitiveDataLogging()
                .UseSnakeCaseNamingConvention();
        }, ServiceLifetime.Transient);
        _serviceProvider = serviceCollection.BuildServiceProvider();
        var context = _serviceProvider.GetService<NLightningContext>() ?? throw new Exception($"Could not find a service provider for type {nameof(NLightningContext)}");

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
        var context = _serviceProvider.GetService<NLightningContext>() ?? throw new Exception("Context is null");
        context.Nodes.Count().PrintDump();

        context.Nodes.AddRange(
            new NLightningContext.Node(),
            new NLightningContext.Node());
        context.SaveChanges();
        context.Nodes.Count().PrintDump();
        context.AddRange(
            new NLightningContext.Node(),
            new NLightningContext.Node());
        context.Nodes.Count().PrintDump();
        context.SaveChanges();
        context.Nodes.Count().PrintDump();

        return Task.CompletedTask;
    }
}
#pragma warning restore xUnit1033 // Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture
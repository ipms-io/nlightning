using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack.Text;
using Xunit.Abstractions;

namespace NLightning.Bolts.Tests.Docker;

using Fixtures;
using Models;
using Utils;

#pragma warning disable xUnit1033 // Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture
[Collection("sqlserver")]
public class SqlServerTests
{
    private readonly ServiceProvider _serviceProvider;

    public SqlServerTests(SqlServerFixture fixture, ITestOutputHelper output)
    {
        var sqlServerFixture = fixture;

        Console.SetOut(new TestOutputWriter(output));

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddDbContextFactory<NLightningContext>(
            options =>
                options.UseSqlServer(sqlServerFixture.DbConnectionString, x =>
                    {
                        x.MigrationsAssembly("NLightning.Models.SqlServer");
                    })
                    .EnableSensitiveDataLogging()
                   );
        serviceCollection.AddDbContext<NLightningContext>(x =>
        {
            x.UseNpgsql(sqlServerFixture.DbConnectionString, y =>
            {
                y.MigrationsAssembly("NLightning.Models.SqlServer");
            })
                .EnableSensitiveDataLogging()
                ;
        }, ServiceLifetime.Transient);
        _serviceProvider = serviceCollection.BuildServiceProvider();
        var context = _serviceProvider.GetService<NLightningContext>();
        //SqlServer takes longer to start from scratch, wait until ready.
        while (!context?.Database.CanConnect() ?? true)
        {
            Task.Delay(100).Wait();
        }
        context?.Database.Migrate();
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
        context.Nodes.AddRange(
            new NLightningContext.Node(),
            new NLightningContext.Node());
        context.Nodes.Count().PrintDump();
        context.SaveChanges();
        context.Nodes.Count().PrintDump();

        return Task.CompletedTask;
    }
}
#pragma warning restore xUnit1033 // Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture
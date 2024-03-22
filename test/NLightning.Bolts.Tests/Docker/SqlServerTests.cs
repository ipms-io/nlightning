using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLightning.Models;
using ServiceStack.Text;

namespace NLightning.Bolts.Tests.Docker;

using Fixtures;
using Utils;
using Xunit.Abstractions;

#pragma warning disable xUnit1033 // Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture
[Collection("sqlserver")]
public class SqlServerTests
{
    private readonly SqlServerFixture _sqlServerFixture;
    private readonly ServiceProvider _serviceProvider;

    public SqlServerTests(SqlServerFixture fixture, ITestOutputHelper output)
    {
        _sqlServerFixture = fixture;

        Console.SetOut(new TestOutputWriter(output));

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddDbContextFactory<MockEfContext>(
            options =>
                options.UseSqlServer(_sqlServerFixture.DbConnectionString, x =>
                    {
                        x.MigrationsAssembly("NLightning.Models.SqlServer");
                    })
                    .EnableSensitiveDataLogging()
                   );
        serviceCollection.AddDbContext<MockEfContext>(x =>
        {
            x.UseNpgsql(_sqlServerFixture.DbConnectionString, x =>
            {
                x.MigrationsAssembly("NLightning.Models.SqlServer");
            })
                .EnableSensitiveDataLogging()
                ;
        }, ServiceLifetime.Transient);
        _serviceProvider = serviceCollection.BuildServiceProvider();
        var context = _serviceProvider.GetService<MockEfContext>();
        //SqlServer takes longer to start from scratch, wait until ready.
        while (!context.Database.CanConnect())
        {
            Task.Delay(100).Wait();
        }
        context.Database.Migrate();
    }

    [Fact]
    public void Check_SqlServer_Exists()
    {
        Assert.True(_sqlServerFixture.IsRunning);
    }

    [Fact]
    public async Task TestDb()
    {
        var context = _serviceProvider.GetService<MockEfContext>();
        context.Xs.Count().PrintDump();

        context.Xs.AddRange(
            new MockEfContext.TableX(),
            new MockEfContext.TableX());
        context.SaveChanges();
        context.Xs.Count().PrintDump();
        context.Xs.AddRange(
            new MockEfContext.TableX(),
            new MockEfContext.TableX());
        context.Xs.Count().PrintDump();
        context.SaveChanges();
        context.Xs.Count().PrintDump();
    }

}
#pragma warning restore xUnit1033 // Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture
// using System.Diagnostics;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.DependencyInjection;
// using ServiceStack.Text;
// using Xunit.Abstractions;
//
// namespace NLightning.Integration.Tests.Docker;
//
// using Fixtures;
// using Infrastructure.Persistence.Contexts;
// using Utils;
//
// #pragma warning disable xUnit1033 // Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture
// [Collection("sqlserver")]
// public class SqlServerTests
// {
//     private readonly ServiceProvider _serviceProvider;
//
//     public SqlServerTests(SqlServerFixture fixture, ITestOutputHelper output)
//     {
//         var sqlServerFixture = fixture;
//
//         Console.SetOut(new TestOutputWriter(output));
//
//         var serviceCollection = new ServiceCollection();
//         serviceCollection.AddDbContextFactory<NLightningDbContext>(
//             options =>
//                 options.UseSqlServer(sqlServerFixture.DbConnectionString, x =>
//                     {
//                         x.MigrationsAssembly("NLightning.Infrastructure.Persistence.SqlServer");
//                     })
//                     .EnableSensitiveDataLogging()
//                    );
//         serviceCollection.AddDbContext<NLightningDbContext>(x =>
//         {
//             x.UseNpgsql(sqlServerFixture.DbConnectionString, y =>
//             {
//                 y.MigrationsAssembly("NLightning.Infrastructure.Persistence.SqlServer");
//             })
//                 .EnableSensitiveDataLogging()
//                 ;
//         }, ServiceLifetime.Transient);
//         _serviceProvider = serviceCollection.BuildServiceProvider();
//         var context = _serviceProvider.GetService<NLightningDbContext>() ?? throw new Exception($"Could not find a service provider for type {nameof(NLightningDbContext)}");
//
//         //SqlServer takes longer to start from scratch, wait until ready.
//         while (!context.Database.CanConnect())
//         {
//             Task.Delay(100).Wait();
//             Debug.Print("Still waiting");
//         }
//         context.Database.Migrate();
//     }
//
//     [Fact]
//     public Task TestDb()
//     {
//         var context = _serviceProvider.GetService<NLightningDbContext>() ?? throw new Exception("Context is null");
//         context.Nodes.Count().PrintDump();
//
//         context.Nodes.AddRange(
//             new NLightningDbContext.Node(),
//             new NLightningDbContext.Node());
//         context.SaveChanges();
//         context.Nodes.Count().PrintDump();
//         context.Nodes.AddRange(
//             new NLightningDbContext.Node(),
//             new NLightningDbContext.Node());
//         context.Nodes.Count().PrintDump();
//         context.SaveChanges();
//         context.Nodes.Count().PrintDump();
//
//         return Task.CompletedTask;
//     }
// }
// #pragma warning restore xUnit1033 // Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture
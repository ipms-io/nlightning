using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ServiceStack.Text;
using Xunit.Abstractions;

namespace NLightning.Integration.Tests.Docker;

using Infrastructure.Persistence.Contexts;
using Utils;

#pragma warning disable xUnit1033 // Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture
public class SqliteTests
{
    public SqliteTests(ITestOutputHelper output)
    {
        Console.SetOut(new TestOutputWriter(output));
    }

    [Fact]
    public void TestInMemorySqliteQuickContext()
    {
        // Create and open a connection. This creates the SQLite in-memory database, which will persist until the connection is closed
        // at the end of the test (see Dispose below).
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var contextOptions = new DbContextOptionsBuilder<NLightningContext>()
            .UseSqlite(connection, x =>
            {
                x.MigrationsAssembly("NLightning.Infrastructure.Persistence.Sqlite");
            })
            .Options;

        // Create the schema and seed some data
        using var context = new NLightningContext(contextOptions);
        context.Database.Migrate();
        // context.Database.EnsureDeleted();
        // context.Database.EnsureCreated();

        context.Nodes.Count().PrintDump();

        context.AddRange(
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
    }
}
#pragma warning restore xUnit1033 // Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NLightning.Models;
using ServiceStack.Text;
using Xunit.Abstractions;

namespace NLightning.Bolts.Tests.Docker;
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

        var contextOptions = new DbContextOptionsBuilder<MockEfContext>()
            .UseSqlite(connection, x =>
            {
                x.MigrationsAssembly("NLightning.Models.Sqlite");
            })
            .Options;

        // Create the schema and seed some data
        using var context = new MockEfContext(contextOptions);
        context.Database.Migrate();
        // context.Database.EnsureDeleted();
        // context.Database.EnsureCreated();

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
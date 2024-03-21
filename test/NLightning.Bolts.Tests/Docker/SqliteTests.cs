using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var contextOptions = new DbContextOptionsBuilder<QuickContext>()
            .UseSqlite(connection)
            .Options;

        // Create the schema and seed some data
        using var context = new QuickContext(contextOptions);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        context.Xs.Count().PrintDump();

        context.AddRange(
            new QuickContext.X(),
            new QuickContext.X());
        context.SaveChanges();
        context.Xs.Count().PrintDump();
        context.AddRange(
            new QuickContext.X(),
            new QuickContext.X());
        context.Xs.Count().PrintDump();
        context.SaveChanges();
        context.Xs.Count().PrintDump();
    }
}
#pragma warning restore xUnit1033 // Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture

public class QuickContext : DbContext
{
    public QuickContext(DbContextOptions<QuickContext> options)
        : base(options)
    {
    }

    public DbSet<X> Xs { get; set; }

    public class X
    {
        public int Id { get; set; }
    }
}
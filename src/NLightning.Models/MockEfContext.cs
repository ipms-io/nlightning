using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NLightning.Models;

public class MockEfContext : DbContext
{
    public MockEfContext(DbContextOptions<MockEfContext> options)
        : base(options)
    {
    }

    public DbSet<TableX> Xs { get; set; }

    public class TableX
    {
        public int Id { get; set; }
        // public string? ShowCasingTransform { get; set; }
        // public bool? ThirdBool { get; set; }
    }
}

//
// public class SqliteMockEfContext : MockEfContext
// { 
//     public SqliteMockEfContext(DbContextOptions<MockEfContext> options)
//         : base(options)
//     {
//     }
//     
// }
//

public class MockEfContextFactory : IDesignTimeDbContextFactory<MockEfContext>
{
    public MockEfContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MockEfContext>();

        var postgresString = Environment.GetEnvironmentVariable("NLIGHTNING_POSTGRES");
        if (postgresString != null)
        {
            optionsBuilder.UseNpgsql(postgresString, x =>
                {
                    x.MigrationsAssembly("NLightning.Models.Postgres");
                })
                .EnableSensitiveDataLogging()
                .UseSnakeCaseNamingConvention();
            return new MockEfContext(optionsBuilder.Options);
        }

        var connectionString = Environment.GetEnvironmentVariable("NLIGHTNING_SQLITE");
        if (connectionString != null)
        {
            optionsBuilder.UseSqlite(connectionString, x =>
            {
                x.MigrationsAssembly("NLightning.Models.Sqlite");
            });
            return new MockEfContext(optionsBuilder.Options);
        }

        throw new Exception("Must set NLIGHTNING_POSTGRES or NLIGHTNING_SQLITE env for generation.");
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NLightning.Models;

public class NLightningContext : DbContext
{
    public NLightningContext(DbContextOptions<NLightningContext> options)
        : base(options)
    {
    }

    public DbSet<Node> Nodes { get; set; }

    [PrimaryKey(nameof(Id))]
    public class Node
    {
        public long Id { get; set; }
    }
}

/// <summary>
/// This is used for dotnet ef CLI to setup connection for migration stuff
/// </summary>
public class NLightningContextFactory : IDesignTimeDbContextFactory<NLightningContext>
{
    public NLightningContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NLightningContext>();

        var postgresString = Environment.GetEnvironmentVariable("NLIGHTNING_POSTGRES");
        if (postgresString != null)
        {
            optionsBuilder.UseNpgsql(postgresString, x =>
                {
                    x.MigrationsAssembly("NLightning.Models.Postgres");
                })
                .EnableSensitiveDataLogging()
                .UseSnakeCaseNamingConvention();
            return new NLightningContext(optionsBuilder.Options);
        }

        var sqlite = Environment.GetEnvironmentVariable("NLIGHTNING_SQLITE");
        if (sqlite != null)
        {
            optionsBuilder.UseSqlite(sqlite, x =>
            {
                x.MigrationsAssembly("NLightning.Models.Sqlite");
            });
            return new NLightningContext(optionsBuilder.Options);
        }

        var sqlServer = Environment.GetEnvironmentVariable("NLIGHTNING_SQLSERVER");
        if (sqlServer != null)
        {
            optionsBuilder.UseSqlServer(sqlServer, x =>
            {
                x.MigrationsAssembly("NLightning.Models.SqlServer");
            });
            return new NLightningContext(optionsBuilder.Options);
        }

        throw new Exception("Must set NLIGHTNING_POSTGRES or NLIGHTNING_SQLITE or NLIGHTNING_SQLSERVER env for generation.");
    }
}
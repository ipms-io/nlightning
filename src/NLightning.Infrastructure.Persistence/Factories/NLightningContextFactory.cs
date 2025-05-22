using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NLightning.Infrastructure.Persistence.Factories;

using Contexts;

/// <summary>
/// This is used for dotnet ef CLI to set up connection for migration stuff
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
                    x.MigrationsAssembly("NLightning.Infrastructure.Persistence.Postgres");
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
                x.MigrationsAssembly("NLightning.Infrastructure.Persistence.Sqlite");
            });
            return new NLightningContext(optionsBuilder.Options);
        }

        var sqlServer = Environment.GetEnvironmentVariable("NLIGHTNING_SQLSERVER");
        if (sqlServer != null)
        {
            optionsBuilder.UseSqlServer(sqlServer, x =>
            {
                x.MigrationsAssembly("NLightning.Infrastructure.Persistence.SqlServer");
            });
            return new NLightningContext(optionsBuilder.Options);
        }

        throw new Exception("Must set NLIGHTNING_POSTGRES or NLIGHTNING_SQLITE or NLIGHTNING_SQLSERVER env for generation.");
    }
}
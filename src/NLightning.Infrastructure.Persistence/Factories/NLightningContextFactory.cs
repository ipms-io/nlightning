using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NLightning.Infrastructure.Persistence.Factories;

using Contexts;
using Enums;

/// <summary>
/// This is used for dotnet ef CLI to set up connection for migration stuff
/// </summary>
public class NLightningContextFactory : IDesignTimeDbContextFactory<NLightningDbContext>
{
    public NLightningDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NLightningDbContext>();

        var postgresString = Environment.GetEnvironmentVariable("NLIGHTNING_POSTGRES");
        if (postgresString != null)
        {
            optionsBuilder.UseNpgsql(postgresString, x =>
                {
                    x.MigrationsAssembly("NLightning.Infrastructure.Persistence.Postgres");
                })
                .EnableSensitiveDataLogging()
                .UseSnakeCaseNamingConvention();
            return new NLightningDbContext(optionsBuilder.Options, DatabaseType.PostgreSql);
        }

        var sqlite = Environment.GetEnvironmentVariable("NLIGHTNING_SQLITE");
        if (sqlite != null)
        {
            optionsBuilder.UseSqlite(sqlite, x =>
            {
                x.MigrationsAssembly("NLightning.Infrastructure.Persistence.Sqlite");
            });
            return new NLightningDbContext(optionsBuilder.Options, DatabaseType.Sqlite);
        }

        var sqlServer = Environment.GetEnvironmentVariable("NLIGHTNING_SQLSERVER");
        if (sqlServer != null)
        {
            optionsBuilder.UseSqlServer(sqlServer, x =>
            {
                x.MigrationsAssembly("NLightning.Infrastructure.Persistence.SqlServer");
            });
            return new NLightningDbContext(optionsBuilder.Options, DatabaseType.MicrosoftSql);
        }

        throw new Exception("Must set NLIGHTNING_POSTGRES or NLIGHTNING_SQLITE or NLIGHTNING_SQLSERVER env for generation.");
    }
}
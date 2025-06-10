namespace NLightning.Infrastructure.Persistence.Providers;

using Enums;

public class DatabaseTypeProvider
{
    public DatabaseType DatabaseType { get; }

    public DatabaseTypeProvider(DatabaseType databaseType)
    {
        DatabaseType = databaseType;
    }
}
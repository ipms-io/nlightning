## NLightning EF Core Models

| **Supported Databases** | ENV for Connection String |
|-------------------------|---------------------------|
| Postgres                | `NLIGHTNING_POSTGRES`     |
| Sqlite                  | `NLIGHTNING_SQLITE`       |
| Sql Server 2016+        | `NLIGHTNING_SQLSERVER`    | 

### Tooling

- Run in `NLightning.Infrastructure.Persistence` directory
- Remember you **MUST** manually remove `DbContext` fields if you are running `remove_migration.sh` migration
- Postgres is run on port 15432 under `postgres_ef_gen` name so one can run unit-tests without blowing out DB.
- Set ENV var if you want to override database to point to, otherwise will spin up empty Postgres and memory db for
  Sqlite
- Name your Migration CamelCased to pass `dotnet format` validation

| Task                  | Command                                       |
 |-----------------------|-----------------------------------------------|
| Add migration         | `./scripts/add_migration.sh AddingFeatureXYZ` |
| Remove last migration | `./scripts/remove_migration.sh`               |
| Startup Postgres      | `./scripts/start_postgres.sh`                 |
| Stop Postgres         | `./scripts/destroy_postgres.sh`               |


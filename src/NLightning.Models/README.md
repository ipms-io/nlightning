## NLightning EF Core Models

| **Supported Databases** | ENV for Connection String | ContextName           |
|-------------|--------|-----------------------|
| Postgres   | NLIGHTNING_POSTGRES | PostgresMockEfContext |
| Sqlite     | NLIGHTNING_SQLITE   | SqliteMockEfContext   |


### Tooling 
- Run in `NLightning.Models` directory
- MUST manually remove `DbContext` fields if you are running `remove_migration.sh` migration
- Postgres is run on port 15432 under `postgres_ef_gen` name so one can run unit-tests without blowing out DB.
- Set ENV var if you want to override database to point to, otherwise will spin up empty Postgres and memory db for Sqlite
- Name your Migrartion CamelCased to pass `dotnet format` validation
 | Task | Command |
 |-----------------------|---------------------------------------|
 | Add migration | `./add_migration.sh AddingFeatureXYZ` |
 | Remove last migration | `./remove_migration.sh`               |
 | Startup Postgres | `./start_postgres.sh`                 |
 | Stop Postgres | `./destroy_postgres.sh`               |


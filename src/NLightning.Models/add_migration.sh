#!/bin/bash

MigrationName=$1
echo "Postgres"
NLIGHTNING_POSTGRES=${NLIGHTNING_POSTGRES:-'User ID=superuser;Password=superuser;Server=localhost;Port=15432;Database=nlightning;'} \
 dotnet ef migrations add $MigrationName \
    --project ../NLightning.Models.Postgres  
    
echo "Sqlite"
NLIGHTNING_SQLITE=${NLIGHTNING_SQLITE:-'Data Source=:memory:'} \
 dotnet ef migrations add $MigrationName \
    --project ../NLightning.Models.Sqlite  
    
echo "SqlServer"
NLIGHTNING_SQLSERVER=${NLIGHTNING_SQLSERVER:-'Server=localhost;Database=nlightning;User Id=sa;Password=Superuser1234*;'} \
 dotnet ef migrations add $MigrationName \
    --project ../NLightning.Models.SqlServer
   
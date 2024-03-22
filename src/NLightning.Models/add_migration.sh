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
   
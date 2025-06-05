docker run --rm -d --name sql_ef_gen -p 1433:1433 \
                -e "ACCEPT_EULA=Y" \
                -e "MSSQL_SA_PASSWORD=Superuser1234*" \
                --platform linux/amd64 \
                mcr.microsoft.com/mssql/server:2022-latest
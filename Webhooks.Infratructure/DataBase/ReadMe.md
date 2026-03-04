
# To create new(er) migration

```bash
dotnet ef migrations add <MIGRATION_NAME> --project Webhooks.Infrastructure\Webhooks.Infrastructure.csproj --startup-project Webhooks.Api\Webhooks.Api.csproj --framework net10.0 --output-dir DataBase\Migrations
```
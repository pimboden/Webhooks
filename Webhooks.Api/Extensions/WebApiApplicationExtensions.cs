using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Webhooks.Infrastructure.Data;

namespace Webhooks.Api.Extensions;

public static class WebApiApplicationExtensions
{
    extension(WebApplication app)
    {
        public async Task ApplyMigrationsAsync(CancellationToken cancellationToken = default)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WebhooksDbContext>();
            var creator = dbContext.GetService<IRelationalDatabaseCreator>();
            if (!await creator.ExistsAsync(cancellationToken))
            {
                await creator.CreateAsync(cancellationToken);
            }
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
    }
}
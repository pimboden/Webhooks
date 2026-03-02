using Microsoft.EntityFrameworkCore;
using Webhooks.Api.Data;

namespace Webhooks.Api.Extensions;

public static class WebApiApplicationExtensions
{
    extension(WebApplication app)
    {
        public async Task ApplyMigrationsAsync(CancellationToken cancellationToken = default)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WebhooksDbContext>();
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
    }
}
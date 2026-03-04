using Microsoft.EntityFrameworkCore;
using Webhooks.Infrastructure.Data;


namespace Webhooks.Api.Endpoints.Webhooks.Subscriptions;

public class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/webhooks/subscriptions", async (
                WebhooksDbContext webhooksDbContext,
                CancellationToken cancellationToken) =>
            {
                var subscriptions = await webhooksDbContext.WebhookSubscriptions
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                return Results.Ok(subscriptions);
            })
            .WithTags("Webhooks")
            .AllowAnonymous();
    }
}
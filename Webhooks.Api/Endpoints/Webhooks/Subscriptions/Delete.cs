using Microsoft.EntityFrameworkCore;
using Webhooks.Infrastructure.Data;

namespace Webhooks.Api.Endpoints.Webhooks.Subscriptions;

public class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/webhooks/subscriptions/{id:guid}", async (
                Guid id,
                WebhooksDbContext webhooksDbContext,
                CancellationToken cancellationToken) =>
            {
                var webhookSubscription = await webhooksDbContext.WebhookSubscriptions
                    .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

                if (webhookSubscription is null)
                {
                    return Results.NotFound(new { Message = $"Webhook subscription with ID {id} not found." });
                }

                webhooksDbContext.WebhookSubscriptions.Remove(webhookSubscription);
                await webhooksDbContext.SaveChangesAsync(cancellationToken);

                return Results.NoContent();
            })
            .WithTags("Webhooks")
            .AllowAnonymous();
    }
}
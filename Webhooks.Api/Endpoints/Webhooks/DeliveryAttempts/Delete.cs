using Microsoft.EntityFrameworkCore;
using Webhooks.Infrastructure.Data;

namespace Webhooks.Api.Endpoints.Webhooks.DeliveryAttempts;

public class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/webhooks/delivery-attempts/{id:guid}", async (
                Guid id,
                WebhooksDbContext webhooksDbContext,
                CancellationToken cancellationToken) =>
            {
                var deliveryAttempt = await webhooksDbContext.WebhookDeliveryAttempts
                    .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

                if (deliveryAttempt is null)
                {
                    return Results.NotFound(new { Message = $"Webhook delivery attempt with ID {id} not found." });
                }

                webhooksDbContext.WebhookDeliveryAttempts.Remove(deliveryAttempt);
                await webhooksDbContext.SaveChangesAsync(cancellationToken);

                return Results.NoContent();
            })
            .WithTags("Webhooks")
            .AllowAnonymous();
    }
}
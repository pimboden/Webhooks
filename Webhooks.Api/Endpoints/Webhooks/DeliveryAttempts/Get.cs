using Microsoft.EntityFrameworkCore;
using Webhooks.Infrastructure.Data;

namespace Webhooks.Api.Endpoints.Webhooks.DeliveryAttempts;

public class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/webhooks/delivery-attempts", async (
                WebhooksDbContext webhooksDbContext,
                CancellationToken cancellationToken) =>
            {
                var deliveryAttempts = await webhooksDbContext.WebhookDeliveryAttempts
                    .AsNoTracking()
                    .OrderByDescending(d => d.Timestamp)
                    .ToListAsync(cancellationToken);

                return Results.Ok(deliveryAttempts);
            })
            .WithTags("Webhooks")
            .AllowAnonymous();
    }
}
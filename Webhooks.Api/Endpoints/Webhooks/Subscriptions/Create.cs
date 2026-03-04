using Webhooks.Api.Models.Requests;
using Webhooks.Infrastructure.Data;
using Webhooks.Infrastructure.Models;

namespace Webhooks.Api.Endpoints.Webhooks.Subscriptions;

public class Create : IEndpoint
{

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/webhooks/subscriptions", async (
                WebhookSubscriptionCreateRequest request,
                WebhooksDbContext webhooksDbContext,
                CancellationToken cancellationToken) =>
            {

                //This is a POC for now no Clean architecture, we will refactor this later, the main goal is to have a working endpoint that we can test with the webhook system, and then we will refactor it to follow Clean Architecture principles.
                var webhookSubscription = new WebhookSubscription(Guid.NewGuid(), request.EventType, request.WebhookUrl, DateTime.UtcNow);
                await webhooksDbContext.WebhookSubscriptions.AddAsync(webhookSubscription, cancellationToken);
                await webhooksDbContext.SaveChangesAsync(cancellationToken);
                return Results.Ok(webhookSubscription);
            })
            .WithTags("SampleData")
            .AllowAnonymous();
    }
}
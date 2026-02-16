using Webhooks.Api.Models;
using Webhooks.Api.Models.Requests;
using Webhooks.Api.Repositories;

namespace Webhooks.Api.Endpoints.Webhooks.Subscriptions;

public class Create : IEndpoint
{

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/webhooks/subscriptions", async (
                WebhookSubscriptionCreateRequest request,
                InMemoryWebhookSubscriptionRepository webhookSubscriptionRepository,
                CancellationToken cancellationToken) =>
            {

                //This is a POC for now no Clean architecture, we will refactor this later, the main goal is to have a working endpoint that we can test with the webhook system, and then we will refactor it to follow Clean Architecture principles.
                var webhookSubscription = new WebhookSubscription(Guid.NewGuid(), request.EventType, request.WebhookUrl, DateTime.UtcNow);
                await webhookSubscriptionRepository.AddAsync(webhookSubscription, cancellationToken);
                return Results.Ok(webhookSubscription);
            })
            .WithTags("SampleData")
            .AllowAnonymous();
    }
}

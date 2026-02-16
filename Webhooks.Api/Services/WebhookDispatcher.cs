using System.Runtime.InteropServices.JavaScript;
using Webhooks.Api.Repositories;

namespace Webhooks.Api.Services;

internal sealed class WebhookDispatcher(HttpClient httpClient,InMemoryWebhookSubscriptionRepository subscriptionRepository)
{
    public async Task DispatchAsync(string eventType, object payload, CancellationToken cancellationToken)
    {
        var subscriptions = await subscriptionRepository.GetByEventType(eventType, cancellationToken);
        foreach (var subscription in subscriptions)
        {
            var request = new
            {
                Id = Guid.NewGuid(),
                subscription.EventType,
                SubscriptionId = subscription.Id,
                Timestamp = DateTime.UtcNow,
                Data = payload
            };
            await httpClient.PostAsJsonAsync(subscription.WebhookUrl, payload, cancellationToken);
        }
    }
}
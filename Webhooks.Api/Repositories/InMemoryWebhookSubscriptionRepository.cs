using System.Collections.ObjectModel;
using Webhooks.Api.Models;

namespace Webhooks.Api.Repositories;

public class InMemoryWebhookSubscriptionRepository
{
    private readonly List<WebhookSubscription> _subscriptions = [];

    public Task AddAsync(WebhookSubscription webhookSubscription, CancellationToken cancellationToken)
    {
        _subscriptions.Add(webhookSubscription);
        return Task.CompletedTask;
    }

    public Task<ReadOnlyCollection<WebhookSubscription>> GetByEventType(string eventType, CancellationToken cancellationToken)
    {
        return Task.FromResult(_subscriptions.Where(s=> string.Equals(s.EventType, eventType, StringComparison.InvariantCultureIgnoreCase)).ToList().AsReadOnly());
    }
}
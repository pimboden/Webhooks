namespace Webhooks.Infratructure.Models;

public sealed record WebhookSubscription(Guid Id,string EventType, string WebhookUrl, DateTime CreateTimeUtc);
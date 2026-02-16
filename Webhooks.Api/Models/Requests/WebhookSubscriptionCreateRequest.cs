namespace Webhooks.Api.Models.Requests;

public sealed record WebhookSubscriptionCreateRequest( string EventType, string WebhookUrl);
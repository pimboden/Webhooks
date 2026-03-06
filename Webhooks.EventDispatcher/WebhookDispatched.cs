namespace Webhooks.EventDispatcher;

/// <summary>
/// The message published to the message bus when a webhook event is dispatched.
/// Consumed by the webhook processing service to fan-out to subscribers.
/// </summary>
public sealed record WebhookDispatched(string EventType, object Data);

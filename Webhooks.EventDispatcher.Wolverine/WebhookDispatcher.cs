using Wolverine;

namespace Webhooks.EventDispatcher.Wolverine;

internal sealed class WebhookDispatcher(IMessageBus bus) : IWebhookDispatcher
{
    public async Task DispatchAsync<T>(string eventType, T data, CancellationToken cancellationToken = default)
        where T : notnull
    {
        using var activity = EventDispatcherDiagnostics.ActivitySource.StartActivity($"{eventType} dispatch webhook");
        activity?.AddTag("event.type", eventType);
        await bus.PublishAsync(new WebhookDispatched(eventType, data));
    }
}

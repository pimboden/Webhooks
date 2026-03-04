using Webhooks.Api.OpenTelemetry;
using Webhooks.Contracts;
using Wolverine;

namespace Webhooks.Api.Services;

internal sealed class WebhookDispatcher(IMessageBus bus)
{
    public async Task DispatchAsync<T>(string eventType, T data, CancellationToken cancellationToken) where T : notnull
    {
        using var activity = DiagnosticConfig.Source.StartActivity($"{eventType} dispatch webhook");
        activity?.AddTag("event.type", eventType);
        await bus.PublishAsync(new WebhookDispatched(eventType, data));
    }
}
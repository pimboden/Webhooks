using MassTransit;
using Webhooks.Api.OpenTelemetry;
using Webhooks.Contracts;

namespace Webhooks.Api.Services;

internal sealed class WebhookDispatcher(IPublishEndpoint publishEndpoint)
{
    public async Task DispatchAsync<T>(string eventType, T data, CancellationToken cancellationToken) where T : notnull
    {
        using var activity = DiagnosticConfig.Source.StartActivity($"{eventType} dispatch webhook");
        activity?.AddTag("event.type", eventType);
        await publishEndpoint.Publish(new WebhookDispatched(eventType, data), cancellationToken);
    }
}
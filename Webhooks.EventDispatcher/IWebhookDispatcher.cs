namespace Webhooks.EventDispatcher;

public interface IWebhookDispatcher
{
    Task DispatchAsync<T>(string eventType, T data, CancellationToken cancellationToken = default)
        where T : notnull;
}

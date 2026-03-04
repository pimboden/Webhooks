namespace Webhooks.EventDispatcher.RabbitMQ;

public sealed class EventDispatcherOptions
{
    /// <summary>RabbitMQ connection string, e.g. "amqp://guest:guest@localhost:5672"</summary>
    public string RabbitMqConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Name of the RabbitMQ exchange. Must match the exchange the processing service binds to.
    /// Default matches the Wolverine package default.
    /// </summary>
    public string ExchangeName { get; set; } = "webhook.dispatched.exchange";
}

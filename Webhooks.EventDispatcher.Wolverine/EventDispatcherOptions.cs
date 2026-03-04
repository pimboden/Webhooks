namespace Webhooks.EventDispatcher.Wolverine;

public sealed class EventDispatcherOptions
{
    /// <summary>RabbitMQ connection string, e.g. "amqp://guest:guest@localhost:5672"</summary>
    public string RabbitMqConnectionString { get; set; } = string.Empty;

    /// <summary>Name of the RabbitMQ exchange. Must match the name the processing service binds to.</summary>
    public string ExchangeName { get; set; } = "webhook.dispatched.exchange";

    /// <summary>Name of the RabbitMQ queue the processing service listens on.</summary>
    public string QueueName { get; set; } = "webhook-dispatched";
}

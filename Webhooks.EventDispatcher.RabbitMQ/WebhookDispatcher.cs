using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Webhooks.EventDispatcher.RabbitMQ;

/// <summary>
/// IWebhookDispatcher implementation using RabbitMQ.Client directly.
/// Compatible with .NET Framework 4.7.x and .NET Standard 2.0.
/// Register as Singleton — it holds a long-lived RabbitMQ connection.
/// </summary>
internal sealed class WebhookDispatcher : IWebhookDispatcher, IDisposable
{
    private readonly string _exchangeName;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    internal WebhookDispatcher(EventDispatcherOptions options)
    {
        _exchangeName = options.ExchangeName;
        var factory = new ConnectionFactory { Uri = new Uri(options.RabbitMqConnectionString) };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare the exchange as fanout — must match the type Wolverine creates.
        // Wolverine's DeclareExchange() defaults to fanout; mismatching types causes
        // RabbitMQ to throw PRECONDITION_FAILED (code 406) if the exchange already exists.
        _channel.ExchangeDeclare(
            exchange: _exchangeName,
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false);
    }

    public Task DispatchAsync<T>(string eventType, T data, CancellationToken cancellationToken = default)
        where T : notnull
    {
        var message = new WebhookDispatched(eventType, data);
        var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        var props = _channel.CreateBasicProperties();
        props.ContentType = "application/json";
        props.DeliveryMode = 2; // persistent

        // Wolverine's RabbitMqEnvelopeMapper reads the message type from the built-in
        // AMQP IBasicProperties.Type field (NOT from a custom header). The mapping is:
        //   envelope.MessageType = props.Type   (on receive)
        //   props.Type = envelope.MessageType   (on send)
        // The value must match typeof(WebhookDispatched).FullName so Wolverine can
        // look up the registered handler for "Webhooks.EventDispatcher.WebhookDispatched".
        props.Type = typeof(WebhookDispatched).FullName!;

        _channel.BasicPublish(
            exchange: _exchangeName,
            routingKey: string.Empty,
            basicProperties: props,
            body: body);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}

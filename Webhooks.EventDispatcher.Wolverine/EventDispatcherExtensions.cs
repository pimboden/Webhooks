using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.RabbitMQ;

namespace Webhooks.EventDispatcher.Wolverine;

public static class EventDispatcherExtensions
{
    /// <summary>
    /// Registers <see cref="IWebhookDispatcher"/> and configures Wolverine with RabbitMQ.
    /// </summary>
    /// <example>
    /// builder.Host.AddEventDispatcher(opts =>
    /// {
    ///     opts.RabbitMqConnectionString = builder.Configuration.GetConnectionString("rabbitmq")!;
    /// });
    /// </example>
    public static IHostBuilder AddEventDispatcher(
        this IHostBuilder hostBuilder,
        Action<EventDispatcherOptions> configure)
    {
        var options = new EventDispatcherOptions();
        configure(options);

        return hostBuilder
            .UseWolverine(opts =>
            {
                opts.UseRabbitMq(new Uri(options.RabbitMqConnectionString))
                    .AutoProvision()
                    .DeclareExchange(options.ExchangeName)
                    .BindExchange(options.ExchangeName).ToQueue(options.QueueName);

                opts.PublishMessage<WebhookDispatched>()
                    .ToRabbitExchange(options.ExchangeName);
            })
            .ConfigureServices(services =>
            {
                services.AddScoped<IWebhookDispatcher, WebhookDispatcher>();
            });
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace Webhooks.EventDispatcher.RabbitMQ;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IWebhookDispatcher"/> using a direct RabbitMQ.Client connection.
    /// Use this in .NET Framework 4.7.x or any app that cannot use Wolverine.
    /// </summary>
    /// <example>
    /// // In Startup.cs or Program.cs:
    /// services.AddEventDispatcher(opts =>
    /// {
    ///     opts.RabbitMqConnectionString = Configuration.GetConnectionString("rabbitmq");
    /// });
    ///
    /// // Then inject IWebhookDispatcher anywhere:
    /// await webhookDispatcher.DispatchAsync("sampledata.created", data, cancellationToken);
    /// </example>
    public static IServiceCollection AddEventDispatcher(
        this IServiceCollection services,
        Action<EventDispatcherOptions> configure)
    {
        var options = new EventDispatcherOptions();
        configure(options);

        // Singleton because WebhookDispatcher holds a long-lived RabbitMQ connection
        services.AddSingleton<IWebhookDispatcher>(new WebhookDispatcher(options));
        return services;
    }
}
